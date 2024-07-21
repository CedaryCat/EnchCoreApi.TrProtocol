using EnchCoreApi.TrProtocol.Attributes;
using EnchCoreApi.TrProtocol.Exceptions;
using EnchCoreApi.TrProtocol.Interfaces;
using EnchCoreApi.TrProtocol.SerializeCodeGenerator.Tools;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO.Compression;
using System.Reflection;
using System.Text;
using Terraria;

namespace EnchCoreApi.TrProtocol.SerializeCodeGenerator {

    [Generator(LanguageNames.CSharp)]
    public class SerializeGenerator2 : IIncrementalGenerator {
        private static bool FilterTypes(SyntaxNode syntaxNode, CancellationToken token) {
            if (syntaxNode is TypeDeclarationSyntax td && td.Keyword.ToString() is not "interface" && td.Keyword.ToString() is not "record" && td.BaseList is not null) {
                return true;
            }
            return false;
        }
        private class NetPacketType {
            public NetPacketType(TypeDeclarationSyntax classDeclaration, string name, MemberDataAccessRound[] members) {
                TypeDeclaration = classDeclaration;
                TypeName = name;
                Members = members;
            }
            public readonly TypeDeclarationSyntax TypeDeclaration;
            public bool IsNetPacket;
            public bool IsSubmodel;
            public bool IsAbstract;
            public bool HasExtraData;
            public bool SideDependent;
            public bool LengthDependent;
            public (string? compressLevel, string? bufferSize) CompressData;
            public readonly string TypeName;
            public readonly IReadOnlyList<MemberDataAccessRound> Members;
        }
        private class MemberDataAccessRound {
            public MemberDataAccessRound(MemberDeclarationSyntax memberDeclaration, string name, TypeSyntax type, bool isProp, AttributeListSyntax[] attributeList) :
                this(memberDeclaration, name, type, isProp, attributeList.SelectMany(atts => atts.Attributes)) {

            }
            public MemberDataAccessRound(MemberDeclarationSyntax memberDeclaration, string name, TypeSyntax type, bool isProp, IEnumerable<AttributeSyntax> attributes) {
                MemberDeclaration = memberDeclaration;
                MemberName = name;
                if (type is NullableTypeSyntax nullable) {
                    MemberType = nullable.ElementType;
                    IsNullable = true;
                }
                else {
                    MemberType = type;
                }
                IsProperty = isProp;
                Attributes = attributes.ToArray();
            }
            public readonly MemberDeclarationSyntax MemberDeclaration;
            public readonly string MemberName;
            public readonly TypeSyntax MemberType;
            public readonly bool IsProperty;
            public readonly bool IsNullable;
            public bool IsConditional;
            public readonly IReadOnlyList<AttributeSyntax> Attributes;
            public void EnterArrayRound(string[] indexNames) {
                IndexNamesStack.Push(indexNames);
            }
            public void ExitArrayRound() {
                IndexNamesStack.Pop();
            }
            Stack<string[]> IndexNamesStack = new Stack<string[]>();
            public string[] IndexNames => IndexNamesStack.Peek();
            public bool IsArrayRound => IndexNamesStack.Count > 0;

            Stack<(ITypeSymbol enumType, ITypeSymbol underlyingType)> EnumTypes = new Stack<(ITypeSymbol enumType, ITypeSymbol underlyingType)>();
            public (ITypeSymbol enumType, ITypeSymbol underlyingType) EnumType => EnumTypes.Peek();
            public void EnterEnumRound((ITypeSymbol enumType, ITypeSymbol underlyingType) type) => EnumTypes.Push(type);
            public void ExitEnumRound() => EnumTypes.Pop();
            public bool IsEnumRound => EnumTypes.Count > 0;
        }

        #region Transform type synatx to data
        private static NetPacketType Transform(GeneratorSyntaxContext context, CancellationToken _) {

            var typeDeclaration = (TypeDeclarationSyntax)context.Node;
            return Transform(typeDeclaration);
        }
        private static NetPacketType Transform(TypeDeclarationSyntax typeDeclaration) {
            var members = typeDeclaration.Members.Where(m => m.Modifiers.Any(m => m.Text == "public")).Select(new Func<MemberDeclarationSyntax, IEnumerable<MemberDataAccessRound>>(m => {

                if (m is FieldDeclarationSyntax field && !field.Modifiers.Any(m => m.Text == "const")) {
                    return field.Declaration.Variables.Select(v => new MemberDataAccessRound(field, v.Identifier.Text, field.Declaration.Type, false, field.AttributeLists.ToArray()));
                }
                else if (m is PropertyDeclarationSyntax prop) {
                    if (prop.AccessorList is null) {
                        return Array.Empty<MemberDataAccessRound>();
                    }
                    foreach (var name in new string[] { "get", "set" }) {
                        var access = prop.AccessorList.Accessors.FirstOrDefault(a => a.Keyword.ToString() == name);
                        if (access == null || access.Modifiers.Count != 0) {
                            return Array.Empty<MemberDataAccessRound>();
                        }
                    }
                    return new MemberDataAccessRound[] { new MemberDataAccessRound(prop, prop.Identifier.Text, prop.Type, true, prop.AttributeLists.ToArray()) };
                }
                else {
                    return Array.Empty<MemberDataAccessRound>();
                }

            })).SelectMany(list => list).Where(m => {

                return !m.Attributes.Any(a => a.AttributeMatch<IgnoreSerializeAttribute>());

            }).ToArray();

            return new NetPacketType(typeDeclaration, typeDeclaration.Identifier.Text, members);
        }
        #endregion

        static Dictionary<string, (NetPacketType? abstractPacket, INamedTypeSymbol enumType, string enumProp, List<(NetPacketType packet, string enumMemberName)> implymentedPacket)> knownPackets = new();
        static readonly string[] NeccessaryUsings = new string[] {
            "System.Runtime.CompilerServices",
            "System.Runtime.InteropServices",
            "System.Diagnostics.CodeAnalysis",
            "EnchCoreApi.TrProtocol.Attributes",
            "EnchCoreApi.TrProtocol.Interfaces",
            "EnchCoreApi.TrProtocol.Exceptions",
            "EnchCoreApi.TrProtocol.Models",
        };
        private static void Execute(SourceProductionContext context, (Compilation compilation, ImmutableArray<NetPacketType> packets) data) {
            //if(!Debugger.IsAttached) Debugger.Launch();

            #region Init global info
            Compilation.LoadCompilation(data.compilation);
            var abstractTypesSymbols = Compilation.GetLocalTypesSymbol().Where(t => t.IsAbstract && t.GetAttributes().Any(a => a.AttributeClass?.Name == nameof(AbstractModelAttribute))).ToArray();
            #endregion

            #region Foreach type
            foreach (var packet in data.packets) {

                packet.TypeDeclaration.GetNamespace(out var classes, out var fullNamespace, out var unit);
                if (classes.Length != 1) {
                    context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor("SCG01", "NetPacket definition error", "Netpacket '{0}' should be a non-nested class", "", DiagnosticSeverity.Error, true), packet.TypeDeclaration.GetLocation(), packet.TypeName));
                }

                if (fullNamespace is null) {
                    context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor("SCG02", "Namespace missing", "Namespace of netpacket '{0}' missing", "", DiagnosticSeverity.Error, true), packet.TypeDeclaration.GetLocation(), packet.TypeName));
                }
                var usings = unit?.Usings.Select(u => u.Name.ToString()).ToList() ?? new List<string>();


                var baseType = packet.TypeDeclaration.BaseList?.Types.First() ?? throw new NullReferenceException();

                if (packet.TypeName == nameof(NetPacket)) {
                    packet.IsNetPacket = true;
                }

                if (Compilation.TryGetTypeSymbol(baseType.ToString(), out var sym, fullNamespace, usings)) {
                    if (sym.IsOrInheritFrom(nameof(NetPacket))) {
                        packet.IsNetPacket = true;
                        packet.IsSubmodel = true;
                    }
                    else if (abstractTypesSymbols.Any(abs => sym.IsOrInheritFrom(abs.Name))) {
                        packet.IsNetPacket = false;
                        packet.IsSubmodel = true;
                    }
                }

                packet.IsAbstract = packet.TypeDeclaration.Modifiers.Any(m => m.Text == "abstract");

                List<string> memberNullables = new List<string>();

                try {
                    if (Compilation.TryGetTypeSymbol(packet.TypeName, out var typeSym, fullNamespace, Array.Empty<string>())) {

                        #region Check type definition vaild
                        if (packet.TypeDeclaration.BaseList.Types.Any(t => t.ToString() == nameof(IExtraData))) {

                            if (!packet.IsNetPacket || !typeSym.IsSealed) {
                                throw new DiagnosticException(
                                    Diagnostic.Create(
                                        new DiagnosticDescriptor(
                                            "SCG03",
                                            $"Invaild type definition",
                                            "This interface is only allowed to be inherited by packets of sealed type",
                                            "",
                                            DiagnosticSeverity.Error,
                                            true),
                                        packet.TypeDeclaration.BaseList.Types.First(t => t.ToString() == nameof(IExtraData)).GetLocation()));
                            }

                            packet.HasExtraData = true;
                        }

                        if (typeSym.AllInterfaces.Any(i => i.Name == nameof(ISideDependent))) {
                            packet.SideDependent = true;
                        }

                        if (typeSym.AllInterfaces.Any(t => t.Name == nameof(ILengthDependent))) {
                            packet.LengthDependent = true;
                        }

                        var compressAtt = packet.TypeDeclaration.AttributeLists.SelectMany(list => list.Attributes).FirstOrDefault(a => a.AttributeMatch<CompressAttribute>());
                        if (compressAtt is not null) {
                            if (!packet.LengthDependent) {
                                throw new DiagnosticException(
                                    Diagnostic.Create(
                                        new DiagnosticDescriptor(
                                            "SCG04",
                                            $"Invaild type definition",
                                            $"'{nameof(CompressAttribute)}' only use on types or structs implymented interface '{nameof(ILengthDependent)}'",
                                            "",
                                            DiagnosticSeverity.Error,
                                            true),
                                       compressAtt.GetLocation()));
                            }
                            packet.CompressData = (compressAtt.ArgumentList?.Arguments[0].Expression?.ToString(), compressAtt.ArgumentList?.Arguments[1].Expression?.ToString());
                        }

                        var packetAutoSeri = typeSym.AllInterfaces.Any(t => t.Name == nameof(IAutoSerializableData));
                        var hasSeriInterface = typeSym.AllInterfaces.Any(i => i.Name == nameof(ISerializableData));
                        var packetManualSeri = !packetAutoSeri && hasSeriInterface;

                        if (packet.IsSubmodel != true && !packetAutoSeri && !packetManualSeri) {
                            continue;
                        }

                        AttributeData? basePacketAttrData;
                        INamedTypeSymbol enumType;
                        string identityProp;
                        PropertyDeclarationSyntax idMember;

                        if (typeSym.IsReferenceType && typeSym.BaseType is not null && typeSym.BaseType.Name != "Object") {

                            basePacketAttrData = typeSym.BaseType.GetAttributes().FirstOrDefault(a => a.AttributeClass?.Name == nameof(AbstractModelAttribute));

                            if (basePacketAttrData is null) {
                                throw new DiagnosticException(
                                    Diagnostic.Create(
                                        new DiagnosticDescriptor(
                                            "SCG05",
                                            $"abstract packet definition invaild",
                                            "abstract packet '{0}' should declarate with attribute '{1}'",
                                            "",
                                            DiagnosticSeverity.Error,
                                            true),
                                        packet.TypeDeclaration.GetLocation(),
                                        typeSym.BaseType,
                                        nameof(AbstractModelAttribute)));
                            }

                            enumType = basePacketAttrData.ConstructorArguments[0].Value as INamedTypeSymbol ?? throw new NullReferenceException();
                            identityProp = basePacketAttrData.ConstructorArguments[1].Value as string ?? throw new NullReferenceException();
                            idMember = packet.TypeDeclaration.Members.OfType<PropertyDeclarationSyntax>().FirstOrDefault(p => p.Identifier.Text == identityProp && p.Type.ToString() == enumType.Name);

                            if (!idMember.Modifiers.Any(m => m.Text == "override") || !idMember.Modifiers.Any(m => m.Text == "sealed") || idMember?.ExpressionBody is not ArrowExpressionClauseSyntax arrow || arrow.Expression is not MemberAccessExpressionSyntax enumMember || enumMember.Expression.ToString() != enumType.Name) {

                                throw new DiagnosticException(
                                    Diagnostic.Create(
                                        new DiagnosticDescriptor(
                                            "SCG06",
                                            $"packet definition invaild",
                                            "packet '{0}' should sealed override get accessor of property '{1}' with arrow expression of enum '{2}' constant",
                                            "",
                                            DiagnosticSeverity.Error,
                                            true),
                                        packet.TypeDeclaration.GetLocation(),
                                        packet.TypeName,
                                        identityProp,
                                        enumType));
                            }
                            else {
                                var packedPacket = (packet, enumMember.Name.ToString());
                                if (knownPackets.TryGetValue(enumType.Name, out var sameParentPackets)) {
                                    sameParentPackets.implymentedPacket.Add(packedPacket);
                                }
                                else {
                                    knownPackets.Add(enumType.Name, (null, enumType, identityProp, new() { packedPacket }));
                                }
                            }
                        }

                        if (packet.IsAbstract) {

                            if (!packet.TypeDeclaration.AttributeMatch<AbstractModelAttribute>()) {
                                throw new DiagnosticException(
                                    Diagnostic.Create(
                                        new DiagnosticDescriptor(
                                            "SCG07",
                                            $"abstract packet definition invaild",
                                            "abstract packet '{0}' should declarate with attribute '{1}'",
                                            "",
                                            DiagnosticSeverity.Error,
                                            true),
                                        packet.TypeDeclaration.GetLocation(),
                                        packet.TypeName,
                                        nameof(AbstractModelAttribute)));
                            }

                            if (!packet.TypeDeclaration.BaseList.Types.Any(i => i.ToString() is nameof(IAutoSerializableData))) {
                                throw new DiagnosticException(
                                    Diagnostic.Create(
                                        new DiagnosticDescriptor(
                                            "SCG08",
                                            $"abstract packet definition invaild",
                                            "abstract packet '{0}' should implement interface '{1}'",
                                            "",
                                            DiagnosticSeverity.Error,
                                            true),
                                        packet.TypeDeclaration.BaseList.GetLocation(),
                                        packet.TypeName,
                                        nameof(IAutoSerializableData)));
                            }

                            basePacketAttrData = typeSym.GetAttributes().Single(a => a.AttributeClass?.Name == nameof(AbstractModelAttribute));
                            enumType = basePacketAttrData.ConstructorArguments[0].Value as INamedTypeSymbol ?? throw new NullReferenceException();
                            identityProp = basePacketAttrData.ConstructorArguments[1].Value as string ?? throw new NullReferenceException();
                            idMember = packet.TypeDeclaration.Members.OfType<PropertyDeclarationSyntax>().FirstOrDefault(p => p.Identifier.Text == identityProp && p.Type.ToString() == enumType.Name);

                            AccessorDeclarationSyntax accessor_get;

                            if (idMember is null || idMember.AccessorList is null || (accessor_get = idMember.AccessorList.Accessors.FirstOrDefault(a => a.Keyword.Text == "get")) is null) {
                                throw new DiagnosticException(
                                    Diagnostic.Create(
                                        new DiagnosticDescriptor(
                                            "SCG09",
                                            $"abstract packet definition invaild",
                                            "abstract packet '{0}' should define a abstract property '{1}' of enum '{2}'",
                                            "",
                                            DiagnosticSeverity.Error,
                                            true),
                                        packet.TypeDeclaration.GetLocation(),
                                        packet.TypeName,
                                        identityProp,
                                        enumType));
                            }
                            else if (idMember.AccessorList.Accessors.Count != 1 || !idMember.Modifiers.Any(m => m.Text == "abstract") || accessor_get.ExpressionBody is not null || accessor_get.Body is not null) {
                                throw new DiagnosticException(
                                    Diagnostic.Create(
                                        new DiagnosticDescriptor(
                                            "SCG10",
                                            $"abstract packet definition invaild",
                                            "abstract packet '{0}' should define a abstract property '{1}' of enum '{2}' with only get accessor",
                                            "",
                                            DiagnosticSeverity.Error,
                                            true),
                                        packet.TypeDeclaration.GetLocation(),
                                        packet.TypeName,
                                        identityProp,
                                        enumType));
                            }

                            if (knownPackets.TryGetValue(enumType.Name, out var packetsWithSameID)) {
                                packetsWithSameID.abstractPacket = packet;
                                knownPackets[enumType.Name] = packetsWithSameID;
                            }
                            else {
                                knownPackets.Add(enumType.Name, (packet, enumType, identityProp, new()));
                            }

                            continue;
                        }
                        #endregion

                        #region Method:Resolve member symbol <CheckMemberSymbol>
                        static void CheckMemberSymbol(INamedTypeSymbol typeSym, MemberDataAccessRound m, out ITypeSymbol mTypeSym, out IFieldSymbol? fieldMemberSym, out IPropertySymbol? propMemberSym) {

                            var fieldsSym = typeSym.GetMembers().OfType<IFieldSymbol>().Where(f => f.DeclaredAccessibility is Accessibility.Public).ToArray();
                            var propertiesSym = typeSym.GetMembers().OfType<IPropertySymbol>().Where(p => p.DeclaredAccessibility is Accessibility.Public).ToArray();

                            propMemberSym = propertiesSym.FirstOrDefault(p => p.Name == m.MemberName);
                            fieldMemberSym = fieldsSym.FirstOrDefault(f => f.Name == m.MemberName);

                            if (fieldMemberSym is not null && !m.IsProperty) {
                                mTypeSym = fieldMemberSym.Type;
                            }
                            else if (propMemberSym is not null && m.IsProperty) {
                                mTypeSym = propMemberSym.Type;
                            }
                            else {
                                throw new DiagnosticException(
                                    Diagnostic.Create(
                                        new DiagnosticDescriptor(
                                            "SCG11",
                                            "unexcepted member definition missing",
                                            "The member '{0}' of type '{1}' cannot be found in compilation",
                                            "",
                                            DiagnosticSeverity.Error,
                                            true),
                                        m.MemberDeclaration.GetLocation(),
                                        m.MemberName,
                                        typeSym.Name));
                            }
                            if (mTypeSym.Name is nameof(Nullable<byte>)) {
                                throw new DiagnosticException(
                                    Diagnostic.Create(
                                        new DiagnosticDescriptor(
                                            "SCG12",
                                            "invaild member definition",
                                            "Members '{0}' of type '{0}' cannot be null-assignable value types '{2}'",
                                            "",
                                            DiagnosticSeverity.Error,
                                            true),
                                        m.MemberType.GetLocation(),
                                        m.MemberName,
                                        typeSym.Name,
                                        m.MemberType));
                            }
                        }
                        #endregion

                        var file = new SourceCodeWriter2(1024 * 4);
                        file.WriteLine();
                        file.WriteLine("// <auto-generated>");
                        var usingTarget = file.WriteLine();
                        file.Write($"namespace {fullNamespace} ");
                        file.BlockWrite((source) => {
                            source.Write($"public unsafe partial {(typeSym.IsValueType ? "struct" : "class")} {packet.TypeName} ");
                            source.BlockWrite((classNode) => {

                                #region Manage type constructors and special behavior

                                List<(string memberName, string memberType)> externalMembers = packet.TypeDeclaration.Members
                                .Where(m => m.AttributeLists
                                .SelectMany(a => a.Attributes)
                                .Any(a => a.AttributeMatch<ExternalMemberAttribute>()))
                                .Select((m) => {
                                    if (m is PropertyDeclarationSyntax prop) {
                                        return new List<(string, string)>() { (prop.Identifier.ToString(), prop.Type.ToString()) };
                                    }
                                    else {
                                        var field = (FieldDeclarationSyntax)m;
                                        var list = new List<(string memberName, string memberType)>();
                                        foreach (var variable in field.Declaration.Variables) {
                                            list.Add((variable.Identifier.ToString(), field.Declaration.Type.ToString()));
                                        }
                                        return list;
                                    }
                                }).SelectMany(m => m).ToList();

                                string externalMemberParams;
                                if (externalMembers.Count == 0) {
                                    externalMemberParams = "";
                                }
                                else {
                                    externalMemberParams = $", {string.Join(", ", externalMembers.Select(m => $"{m.memberType} _{m.memberName} = default"))}";
                                }
                                if (packet.SideDependent) {

                                    if (!packet.IsNetPacket) {
                                        throw new DiagnosticException(
                                            Diagnostic.Create(
                                                new DiagnosticDescriptor(
                                                    "SCG13",
                                                    $"Invaild type definition",
                                                    $"This interface '{nameof(ISideDependent)}' is only allowed to be inherited by packets",
                                                    "",
                                                    DiagnosticSeverity.Error,
                                                    true),
                                                packet.TypeDeclaration.GetLocation()));
                                    }
                                    classNode.WriteLine($"public bool {nameof(ISideDependent.IsServerSide)} {{ get; set; }}");
                                }

                                if (packet.LengthDependent) {
                                    if (packet.HasExtraData) {
                                        classNode.WriteLine($"public byte[] {nameof(IExtraData.ExtraData)} {{ get; set; }} = Array.Empty<byte>();");
                                    }
                                    classNode.Write($"public {packet.TypeName}(ref void* ptr, void* ptr_end{(packet.SideDependent ? ", bool isServerSide" : "")}{externalMemberParams})");
                                    classNode.BlockWrite((source) => {
                                        source.WriteLine($"ReadContent(ref ptr, ptr_end);");
                                    });
                                }
                                else {
                                    classNode.Write($"public {packet.TypeName}(ref void* ptr{(packet.SideDependent ? ", bool isServerSide" : "")}{externalMemberParams})");
                                    classNode.BlockWrite((source) => {
                                        if (packet.SideDependent) {
                                            source.WriteLine($"{nameof(ISideDependent.IsServerSide)} = isServerSide;");
                                        }
                                        foreach (var m in externalMembers) {
                                            source.WriteLine($"{m.memberName} = _{m.memberName};");
                                        }
                                        source.WriteLine($"ReadContent(ref ptr);");
                                    });
                                }

                                if (packetAutoSeri) {
                                    List<MemberDataAccessRound> defaults = new(packet.Members.Count);
                                    var initMembers = packet.Members.Where(m => !m.Attributes.Any(a => a.AttributeMatch<InitNullableAttribute>())).ToArray();
                                    int shouldCreateNewConstructor = -1;
                                    var parameters = initMembers.Select(m => {
                                        if (m.Attributes.Any(a => a.AttributeMatch<InitDefaultValueAttribute>())) {
                                            if (shouldCreateNewConstructor == -1) {
                                                shouldCreateNewConstructor = 0;
                                            }
                                            if (m.IsNullable) {
                                                defaults.Add(m);
                                                return null;
                                            }
                                            if (Compilation.TryGetTypeSymbol(m.MemberType.GetTypeSymbolName(), out var mtsym, fullNamespace, usings) && mtsym.IsUnmanagedType) {
                                                defaults.Add(m);
                                                return null;
                                            }
                                        }
                                        if (shouldCreateNewConstructor == 0) {
                                            shouldCreateNewConstructor = 1;
                                        }
                                        return $"{m.MemberType} _{m.MemberName}";
                                    }).Where(s => s is not null).ToList();

                                    var parameters_extra = new List<string>();
                                    if (packet.HasExtraData) {
                                        parameters_extra.Add($"byte[] _{nameof(IExtraData.ExtraData)}");
                                    }
                                    if (packet.SideDependent) {
                                        parameters_extra.Add($"bool _{nameof(ISideDependent.IsServerSide)}");
                                    }
                                    var parameters_defaults = defaults.Select(m => $"{m.MemberType} _{m.MemberName} = default").ToList();

                                    if (shouldCreateNewConstructor == 1 || (parameters_defaults.Count > 0 && (packet.HasExtraData || packet.SideDependent))) {
                                        classNode.Write($"public {packet.TypeName}({string.Join(", ", initMembers.Select(m => $"{m.MemberType} _{m.MemberName}").Concat(parameters_extra))}) ");
                                        classNode.BlockWrite((source) => {
                                            if (packet.SideDependent) {
                                                source.WriteLine($"this.{nameof(ISideDependent.IsServerSide)} = _{nameof(ISideDependent.IsServerSide)};");
                                            }
                                            foreach (var m in initMembers) {
                                                source.WriteLine($"this.{m.MemberName} = _{m.MemberName};");
                                            }
                                            if (packet.HasExtraData) {
                                                source.WriteLine($"this.{nameof(IExtraData.ExtraData)} = _{nameof(IExtraData.ExtraData)};");
                                            }
                                        });
                                    }
                                    classNode.Write($"public {packet.TypeName}({string.Join(", ", parameters.Concat(parameters_extra).Concat(parameters_defaults))}) ");
                                    classNode.BlockWrite((source) => {
                                        if (packet.SideDependent) {
                                            source.WriteLine($"this.{nameof(ISideDependent.IsServerSide)} = _{nameof(ISideDependent.IsServerSide)};");
                                        }
                                        foreach (var m in initMembers) {
                                            source.WriteLine($"this.{m.MemberName} = _{m.MemberName};");
                                        }
                                        if (packet.HasExtraData) {
                                            source.WriteLine($"this.{nameof(IExtraData.ExtraData)} = _{nameof(IExtraData.ExtraData)};");
                                        }
                                    });
                                }

                                if (packetManualSeri) {
                                    return;
                                }
                                #endregion

                                #region Method:Add serialization condition to members <MemberConditionCheck>
                                void MemberConditionCheck(ITypeSymbol parent, MemberDataAccessRound m, ITypeSymbol mType, string? parant_var, BlockNode membersSource, bool seri) {
                                    if (!m.IsArrayRound && !m.IsEnumRound) {

                                        List<string> conditions = new List<string>();

                                        foreach (var conditionGroupAtt in m.MemberDeclaration.AttributeLists) {
                                            List<string> conditionAnd = new List<string>();

                                            foreach (var conditionAtt in conditionGroupAtt.Attributes) {


                                                if (conditionAtt.AttributeMatch<ConditionAttribute>() && conditionAtt.ArgumentList is not null) {
                                                    var conditionArgs = conditionAtt.ArgumentList.Arguments;

                                                    string conditionMemberName;
                                                    string conditionMemberAccess;
                                                    string? conditionIndex = null;
                                                    bool conditionPred = true;

                                                    if (conditionArgs[0].Expression.IsLiteralExpression(out var text1) && text1.StartsWith("\"") && text1.EndsWith("\"")) {
                                                        conditionMemberAccess = (parant_var is null ? "" : $"{parant_var}.") + (conditionMemberName = text1[1..^1]);
                                                    }
                                                    else if (conditionArgs[0].Expression is InvocationExpressionSyntax invo1 && invo1.Expression.ToString() == "nameof") {
                                                        conditionMemberAccess = (parant_var is null ? "" : $"{parant_var}.") + (conditionMemberName = invo1.ArgumentList.Arguments.First().Expression.ToString());
                                                    }
                                                    else {
                                                        goto throwException;
                                                    }
                                                    if (conditionArgs.Count == 3) {
                                                        if (conditionArgs[1].Expression.IsLiteralExpression(out var text3) && byte.TryParse(text3, out _)) {
                                                            conditionIndex = text3;
                                                        }
                                                        else if (conditionArgs[1].Expression is InvocationExpressionSyntax invo2 && invo2.Expression.ToString() == "sizeof") {
                                                            conditionIndex = invo2.ToString();
                                                        }
                                                        else {
                                                            goto throwException;
                                                        }
                                                        if (conditionArgs[2].Expression.IsLiteralExpression(out text3) && bool.TryParse(text3, out var pred)) {
                                                            conditionPred = pred;
                                                        }
                                                        else {
                                                            goto throwException;
                                                        }
                                                    }
                                                    else if (conditionArgs.Count == 2) {
                                                        if (conditionArgs[1].Expression.IsLiteralExpression(out var text2)) {
                                                            if (bool.TryParse(text2, out var pred)) {
                                                                conditionPred = pred;
                                                            }
                                                            else if (byte.TryParse(text2, out _)) {
                                                                conditionIndex = text2;
                                                            }
                                                            else {
                                                                goto throwException;
                                                            }
                                                        }
                                                        else if (conditionArgs[1].Expression is InvocationExpressionSyntax invo2 && invo2.Expression.ToString() == "sizeof") {
                                                            conditionIndex = invo2.ToString();
                                                        }
                                                        else {
                                                            goto throwException;
                                                        }
                                                    }
                                                    else {
                                                        goto throwException;
                                                    }

                                                    var conditionMember = typeSym.GetMembers(conditionMemberName);

                                                    if (conditionIndex is not null && conditionMember.OfType<IFieldSymbol>().Select(f => f.Type).Concat(conditionMember.OfType<IPropertySymbol>().Select(p => p.Type)).Any(t => t.Name != nameof(BitsByte))) {
                                                        throw new DiagnosticException(
                                                            Diagnostic.Create(
                                                                new DiagnosticDescriptor(
                                                                    "SCG14",
                                                                    "condition attribute invaild",
                                                                    "arg1 of condition attribute must be name of field or property which type is {0}.",
                                                                    "",
                                                                    DiagnosticSeverity.Error,
                                                                    true),
                                                                conditionAtt.GetLocation(),
                                                                nameof(BitsByte)));
                                                    }
                                                    if (conditionIndex is null && conditionMember.OfType<IFieldSymbol>().Select(f => f.Type).Concat(conditionMember.OfType<IPropertySymbol>().Select(p => p.Type)).Any(t => t.Name != nameof(Boolean))) {
                                                        throw new DiagnosticException(
                                                            Diagnostic.Create(
                                                                new DiagnosticDescriptor(
                                                                    "SCG15",
                                                                    "condition attribute invaild",
                                                                    "arg1 of condition attribute must be name of field or property which type is {0}.",
                                                                    "",
                                                                    DiagnosticSeverity.Error,
                                                                    true),
                                                                conditionAtt.GetLocation(),
                                                                nameof(Boolean)));
                                                    }

                                                    conditionAnd.Add($"{(conditionPred ? "" : "!")}{conditionMemberAccess}{(conditionIndex is null ? "" : $"[{conditionIndex}]")}");

                                                    continue;

                                                throwException:
                                                    throw new DiagnosticException(
                                                        Diagnostic.Create(
                                                            new DiagnosticDescriptor(
                                                                "SCG16",
                                                                "condition attribute invaild",
                                                                "condition attribute argument of member '{0}' packet '{1}' is invaild.",
                                                                "",
                                                                DiagnosticSeverity.Error,
                                                                true),
                                                            conditionAtt.GetLocation(),
                                                            m.MemberName,
                                                            packet.TypeName));
                                                }

                                                string? condiOperator = (conditionAtt.Name.ToString()) switch {
                                                    nameof(ConditionEqualAttribute) or "ConditionEqual" => "==",
                                                    nameof(ConditionNotEqualAttribute) or "ConditionNotEqual" => "!=",
                                                    nameof(ConditionGreaterThanAttribute) or "ConditionGreaterThan" => ">",
                                                    nameof(ConditionGreaterThanEqualAttribute) or "ConditionGreaterThanEqual" => ">=",
                                                    nameof(ConditionLessThanAttribute) or "ConditionLessThan" => "<",
                                                    nameof(ConditionLessThanEqualAttribute) or "ConditionLessThanEqual" => "<=",
                                                    _ => null,
                                                };

                                                if (condiOperator is not null && conditionAtt.ArgumentList is not null) {
                                                    var conditionArgs = conditionAtt.ArgumentList.Arguments;

                                                    string conditionMemberName;
                                                    string conditionMemberAccess;
                                                    string checkValue;
                                                    if (conditionArgs[0].Expression.IsLiteralExpression(out var text1) && text1.StartsWith("\"") && text1.EndsWith("\"")) {
                                                        conditionMemberAccess = (parant_var is null ? "" : $"{parant_var}.") + (conditionMemberName = text1[1..^1]);
                                                    }
                                                    else if (conditionArgs[0].Expression is InvocationExpressionSyntax invo1 && invo1.Expression.ToString() == "nameof") {
                                                        conditionMemberAccess = (parant_var is null ? "" : $"{parant_var}.") + (conditionMemberName = invo1.ArgumentList.Arguments.First().Expression.ToString());
                                                    }
                                                    else {
                                                        goto throwException;
                                                    }

                                                    if (conditionArgs[1].Expression.IsLiteralExpression(out var text3) && int.TryParse(text3, out _)) {
                                                        checkValue = text3;
                                                    }
                                                    else if (conditionArgs[1].Expression is PrefixUnaryExpressionSyntax pu && pu.OperatorToken.Text == "-" && pu.Operand.IsLiteralExpression(out var text4) && int.TryParse(text4, out _)) {
                                                        checkValue = pu.ToString();
                                                    }
                                                    else if (conditionArgs[1].Expression is InvocationExpressionSyntax invo2 && invo2.Expression.ToString() == "sizeof") {
                                                        checkValue = invo2.ToString();
                                                    }
                                                    else {
                                                        goto throwException;
                                                    }

                                                    var conditionMember = typeSym.GetMembers(conditionMemberName);
                                                    var condiFieldType = conditionMember.OfType<IFieldSymbol>().Select(f => f.Type).Concat(conditionMember.OfType<IPropertySymbol>().Select(p => p.Type)).FirstOrDefault();

                                                    if (condiFieldType is null) {
                                                        throw new DiagnosticException(
                                                            Diagnostic.Create(
                                                                new DiagnosticDescriptor(
                                                                    "SCG17",
                                                                    "condition attribute invaild",
                                                                    "arg1 of condition attribute must be name of field or property",
                                                                    "",
                                                                    DiagnosticSeverity.Error,
                                                                    true),
                                                                conditionAtt.GetLocation()));
                                                    }

                                                    var convertible = condiFieldType.AllInterfaces.Any(i => i.Name is nameof(IConvertible));

                                                    conditionAnd.Add($"{conditionMemberAccess} {condiOperator} {(convertible ? "" : $"({condiFieldType})")}{checkValue}");

                                                    continue;

                                                throwException:
                                                    throw new DiagnosticException(
                                                        Diagnostic.Create(
                                                            new DiagnosticDescriptor(
                                                                "SCG18",
                                                                "condition attribute invaild",
                                                                "condition attribute argument of member '{0}' packet '{1}' is invaild.",
                                                                "",
                                                                DiagnosticSeverity.Error,
                                                                true),
                                                            conditionAtt.GetLocation(),
                                                            m.MemberName,
                                                            packet.TypeName));
                                                }
                                            }

                                            if (conditionAnd.Count == 1) {
                                                conditions.Add(conditionAnd[0]);
                                            }
                                            else if (conditionAnd.Count > 1) {
                                                conditions.Add($"({string.Join(" && ", conditionAnd)})");
                                            }
                                        }

                                        if (conditions.Count != 0) {

                                            m.IsConditional = true;

                                            if (!mType.IsUnmanagedType && mType.NullableAnnotation != NullableAnnotation.Annotated) {

                                                throw new DiagnosticException(
                                                    Diagnostic.Create(
                                                        new DiagnosticDescriptor(
                                                            "SCG19",
                                                            "Array rank size invaild",
                                                            "Reference type member '{0}' marked as conditional serializations must be declared nullable",
                                                            "",
                                                            DiagnosticSeverity.Error,
                                                            true),
                                                        m.MemberType.GetLocation(),
                                                        m.MemberName));
                                            }

                                            membersSource.WarpBlock(($"if ({string.Join(" || ", conditions)}) ", false));
                                        }

                                        string? serverSideConditionOp = null;
                                        AttributeSyntax? another = null;
                                        if (m.Attributes.Any(a => a.AttributeMatch<C2SOnlyAttribute>())) {
                                            serverSideConditionOp = seri ? "!" : "";
                                            another = m.Attributes.FirstOrDefault(a => a.AttributeMatch<S2COnlyAttribute>());
                                        }
                                        else if (m.Attributes.Any(a => a.AttributeMatch<S2COnlyAttribute>())) {
                                            serverSideConditionOp = seri ? "" : "!";
                                            another = m.Attributes.FirstOrDefault(a => a.AttributeMatch<C2SOnlyAttribute>());
                                        }
                                        if (another is not null) {
                                            throw new DiagnosticException(
                                                Diagnostic.Create(
                                                    new DiagnosticDescriptor(
                                                        "SCG03",
                                                        $"Invaild member definition",
                                                        $"Only one of C2SOnly and S2COnly can exist at the same time",
                                                        "",
                                                        DiagnosticSeverity.Error,
                                                        true),
                                                    another.GetLocation()));
                                        }
                                        if (serverSideConditionOp is not null) {
                                            if (!parent.AllInterfaces.Any(i => i.Name == nameof(ISideDependent))) {
                                                throw new DiagnosticException(
                                                Diagnostic.Create(
                                                    new DiagnosticDescriptor(
                                                        "SCG03",
                                                        $"Invaild member definition",
                                                        $"C2SOnly and S2COnly can only be annotated on members of types that implement the ISideDependent interface",
                                                        "",
                                                        DiagnosticSeverity.Error,
                                                        true),
                                                    m.MemberDeclaration.GetLocation()));
                                            }

                                            membersSource.WarpBlock(($"if ({serverSideConditionOp}{nameof(ISideDependent.IsServerSide)}) ", false));
                                        }
                                    }

                                    if (m.IsArrayRound && !m.IsEnumRound) {
                                        var arrConditAtt = m.Attributes.FirstOrDefault(a => a.AttributeMatch<ConditionArrayAttribute>());
                                        if (arrConditAtt is not null && arrConditAtt.ArgumentList is not null) {

                                            if (m.IndexNames.Length != 1) {
                                                throw new DiagnosticException(
                                                    Diagnostic.Create(
                                                        new DiagnosticDescriptor(
                                                            "SCG20",
                                                            "invaild array definition",
                                                            "ArrayConditionAttribute is only allowed to be applied to members of the one-dimensional array type",
                                                            "",
                                                            DiagnosticSeverity.Error,
                                                            true),
                                                        m.MemberType.GetLocation()));
                                            }

                                            var conditionArgs = arrConditAtt.ArgumentList.Arguments;

                                            string conditionMemberName;
                                            string conditionMemberAccess;
                                            string conditionIndex;
                                            bool conditionPred = true;

                                            if (conditionArgs[0].Expression.IsLiteralExpression(out var text1) && text1.StartsWith("\"") && text1.EndsWith("\"")) {
                                                conditionMemberAccess = (parant_var is null ? "" : $"{parant_var}.") + (conditionMemberName = text1[1..^1]);
                                            }
                                            else if (conditionArgs[0].Expression is InvocationExpressionSyntax invo1 && invo1.Expression.ToString() == "nameof") {
                                                conditionMemberAccess = (parant_var is null ? "" : $"{parant_var}.") + (conditionMemberName = invo1.ArgumentList.Arguments.First().Expression.ToString());
                                            }
                                            else {
                                                goto throwException;
                                            }
                                            if (conditionArgs[1].Expression.IsLiteralExpression(out var text3) && byte.TryParse(text3, out _)) {
                                                conditionIndex = text3;
                                            }
                                            else if (conditionArgs[1].Expression is InvocationExpressionSyntax invo2 && invo2.Expression.ToString() == "sizeof") {
                                                conditionIndex = invo2.ToString();
                                            }
                                            else {
                                                goto throwException;
                                            }
                                            if (conditionArgs.Count == 3) {
                                                if (conditionArgs[2].Expression.IsLiteralExpression(out text3) && bool.TryParse(text3, out var pred)) {
                                                    conditionPred = pred;
                                                }
                                                else {
                                                    goto throwException;
                                                }
                                            }

                                            string condiExpression;
                                            if (conditionIndex.ToString() == "0") {
                                                condiExpression = m.IndexNames[0];
                                            }
                                            else {
                                                condiExpression = $"{m.IndexNames[0]} + {conditionIndex}";
                                            }

                                            membersSource.WarpBlock(($"if ({(conditionPred ? "" : "!")}{conditionMemberAccess}[{condiExpression}]) ", false));
                                            return;

                                        throwException:
                                            throw new DiagnosticException(
                                                Diagnostic.Create(
                                                    new DiagnosticDescriptor(
                                                        "SCG21",
                                                        "array condition attribute invaild",
                                                        "array condition attribute argument of member '{0}' packet '{1}' is invaild.",
                                                        "",
                                                        DiagnosticSeverity.Error,
                                                        true),
                                                    arrConditAtt.GetLocation(),
                                                    m.MemberName,
                                                    packet.TypeName));
                                        }
                                    }
                                }
                                #endregion
                                #region Method:

                                static List<(string memberName, string value)> GetExternalDefaultValue(MemberDataAccessRound m, ITypeSymbol memberTypeSym) {
                                    List<(string memberName, string memberValue)> defInnerMemberValueAssigns = new();
                                    foreach (var defMemberValueAttr in m.Attributes.Where(a => a.AttributeMatch<ExternalMemberValueAttribute>())) {
                                        var attrParams = defMemberValueAttr.ExtractAttributeParams();
                                        string memberName;
                                        string value;
                                        if (attrParams[0].IsLiteralExpression(out var text1) && text1.StartsWith("\"") && text1.EndsWith("\"")) {
                                            memberName = text1[1..^1];
                                        }
                                        else if (attrParams[0] is InvocationExpressionSyntax invo1 && invo1.Expression.ToString() == "nameof") {
                                            memberName = invo1.ArgumentList.Arguments.First().Expression.ToString();
                                        }
                                        else {
                                            throw new DiagnosticException(
                                                Diagnostic.Create(
                                                    new DiagnosticDescriptor(
                                                        "SCG16",
                                                        "condition attribute invaild",
                                                        "condition attribute argument of member '{0}' is invaild.",
                                                        "",
                                                        DiagnosticSeverity.Error,
                                                        true),
                                                    defMemberValueAttr.GetLocation(),
                                                    m.MemberName));
                                        }

                                        memberName = memberName.Split('.').Last();

                                        if (attrParams[1].IsLiteralExpression(out var text2)) {
                                            if (text2.StartsWith("\"") && text2.EndsWith("\"")) {
                                                value = text2;
                                            }
                                            else if (bool.TryParse(text2, out var pred)) {
                                                value = pred.ToString().ToLower();
                                            }
                                            else if (long.TryParse(text2, out var num)) {
                                                value = num.ToString();
                                            }
                                            else if (double.TryParse(text2, out var num2)) {
                                                value = num2.ToString();
                                            }
                                            else {
                                                throw new DiagnosticException(
                                                    Diagnostic.Create(
                                                        new DiagnosticDescriptor(
                                                            "SCG16",
                                                            "condition attribute invaild",
                                                            "condition attribute argument of member '{0}' packet is invaild.",
                                                            "",
                                                            DiagnosticSeverity.Error,
                                                            true),
                                                        defMemberValueAttr.GetLocation(),
                                                        m.MemberName));
                                            }
                                        }
                                        else if (attrParams[1] is InvocationExpressionSyntax invo2) {
                                            if (invo2.Expression.ToString() == "nameof") {
                                                value = $"\"{invo2.ArgumentList.Arguments.First().Expression}\"";
                                            }
                                            else if (invo2.Expression.ToString() == "sizeof") {
                                                value = invo2.ToString();
                                            }
                                            else {
                                                throw new DiagnosticException(
                                                    Diagnostic.Create(
                                                        new DiagnosticDescriptor(
                                                            "SCG16",
                                                            "condition attribute invaild",
                                                            "condition attribute argument of member '{0}' packet is invaild.",
                                                            "",
                                                            DiagnosticSeverity.Error,
                                                            true),
                                                        defMemberValueAttr.GetLocation(),
                                                        m.MemberName));
                                            }
                                        }
                                        else {
                                            throw new DiagnosticException(
                                                    Diagnostic.Create(
                                                        new DiagnosticDescriptor(
                                                            "SCG16",
                                                            "condition attribute invaild",
                                                            "condition attribute argument of member '{0}' packet is invaild.",
                                                            "",
                                                            DiagnosticSeverity.Error,
                                                            true),
                                                        defMemberValueAttr.GetLocation(),
                                                        m.MemberName));
                                        }
                                        ITypeSymbol type = memberTypeSym;
                                        if (memberTypeSym is IArrayTypeSymbol arrTypeSym) {
                                            type = arrTypeSym.ElementType;
                                        }
                                        var ms = type.GetMembers();
                                        var innerDVField = type.GetMembers().OfType<IFieldSymbol>().FirstOrDefault(f => f.Name == memberName);
                                        var innerDVProp = type.GetMembers().OfType<IPropertySymbol>().FirstOrDefault(f => f.Name == memberName);

                                        if (innerDVField is null && innerDVProp is null) {
                                            throw new DiagnosticException(
                                                Diagnostic.Create(
                                                    new DiagnosticDescriptor(
                                                        "SCG34",
                                                        "unexcepted member definition",
                                                        "Cannot find the default member value '{0}' in the type {1} of this member '{2}'",
                                                        "",
                                                        DiagnosticSeverity.Error,
                                                        true),
                                                    m.MemberType.GetLocation(),
                                                    memberName,
                                                    type.Name,
                                                    m.MemberName));
                                        }

                                        if (innerDVField is not null) {

                                            if (!innerDVField.GetAttributes().Any(a => a.AttributeClass?.Name == nameof(ExternalMemberAttribute))) {
                                                throw new DiagnosticException(
                                                Diagnostic.Create(
                                                    new DiagnosticDescriptor(
                                                        "SCG35",
                                                        "unexcepted member definition",
                                                        "Only members decorated with {0} can be set as default values externally with {1}",
                                                        "",
                                                        DiagnosticSeverity.Error,
                                                        true),
                                                    m.MemberType.GetLocation(),
                                                    nameof(ExternalMemberAttribute),
                                                    nameof(ExternalMemberValueAttribute)));
                                            }

                                            defInnerMemberValueAssigns.Add((innerDVField.Name, value));
                                        }
                                        else if (innerDVProp is not null) {
                                            if (!innerDVProp.GetAttributes().Any(a => a.AttributeClass?.Name == nameof(ExternalMemberAttribute))) {
                                                throw new DiagnosticException(
                                                Diagnostic.Create(
                                                    new DiagnosticDescriptor(
                                                        "SCG35",
                                                        "unexcepted member definition",
                                                        "Only members decorated with {0} can be set as default values externally with {1}",
                                                        "",
                                                        DiagnosticSeverity.Error,
                                                        true),
                                                    m.MemberType.GetLocation(),
                                                    nameof(ExternalMemberAttribute),
                                                    nameof(ExternalMemberValueAttribute)));
                                            }

                                            defInnerMemberValueAssigns.Add((innerDVProp.Name, value));
                                        }
                                    }
                                    return defInnerMemberValueAssigns;
                                }

                                int indexID = 0;
                                void UnfoldMembers(BlockNode seriNode, BlockNode deserNode, INamedTypeSymbol typeSym, IEnumerable<(MemberDataAccessRound m, string? parant_var)> memberAccesses) {
                                    
                                    try {
                                        foreach (var (m, parant_var) in memberAccesses) {
                                            var mType = m.MemberType;
                                            var mTypeStr = mType.ToString();
                                            CheckMemberSymbol(typeSym, m, out var memberTypeSym, out var fieldMemberSym, out var propMemberSym);

                                            string memberAccess;

                                            if (parant_var == null) {
                                                memberAccess = m.MemberName;
                                            }
                                            else {
                                                memberAccess = $"{parant_var}.{m.MemberName}";

                                                var memberNameSpace = memberTypeSym.GetFullNamespace();
                                                if (!string.IsNullOrEmpty(memberNameSpace) && !usings.Contains(memberNameSpace)) {
                                                    usings.Add(memberNameSpace);
                                                }
                                            }

                                            if (m.IsArrayRound) {
                                                mType = ((ArrayTypeSyntax)mType).ElementType;
                                                memberTypeSym = ((IArrayTypeSymbol)memberTypeSym).ElementType;

                                                if (mType is NullableTypeSyntax) {
                                                    throw new DiagnosticException(
                                                        Diagnostic.Create(
                                                            new DiagnosticDescriptor(
                                                                "SCG22",
                                                                "invaild array element definition",
                                                                "The element type of an array type member '{0}' of type '{1}' cannot be nullable '{2}'",
                                                                "",
                                                                DiagnosticSeverity.Error,
                                                                true),
                                                            mType.GetLocation(),
                                                            m.MemberName,
                                                            packet.TypeName,
                                                            mType.ToString()));
                                                }

                                                mTypeStr = mType.ToString();
                                                memberAccess = $"{memberAccess}[{string.Join(",", m.IndexNames)}]";
                                            }
                                            if (m.IsEnumRound) {
                                                mTypeStr = m.EnumType.underlyingType.GetPredifinedName();
                                            }
                                            var seriMemberBlock = new BlockNode(seriNode);
                                            var deserMemberBlock = new BlockNode(deserNode);


                                            switch (mTypeStr) {
                                                case "byte":
                                                case nameof(Byte):
                                                case "sbyte":
                                                case nameof(SByte):
                                                case "ushort":
                                                case nameof(UInt16):
                                                case "short":
                                                case nameof(Int16):
                                                case "uint":
                                                case nameof(UInt32):
                                                case "int":
                                                case nameof(Int32):
                                                case "ulong":
                                                case nameof(UInt64):
                                                case "long":
                                                case nameof(Int64):
                                                case "float":
                                                case nameof(Single):
                                                case "double":
                                                case nameof(Double):
                                                case "decimal":
                                                case nameof(Decimal):
                                                    seriMemberBlock.WriteLine($"Unsafe.Write(ptr_current, {(m.IsEnumRound? $"({mTypeStr})" + memberAccess : memberAccess)});");
                                                    seriMemberBlock.WriteLine($"ptr_current = Unsafe.Add<{mTypeStr}>(ptr_current, 1);");
                                                    seriMemberBlock.WriteLine();

                                                    deserMemberBlock.WriteLine($"{memberAccess} = {(m.IsEnumRound ? $"({m.EnumType.enumType.Name})" : "")}Unsafe.Read<{mTypeStr}>(ptr_current);");
                                                    deserMemberBlock.WriteLine($"ptr_current = Unsafe.Add<{mTypeStr}>(ptr_current, 1);");
                                                    deserMemberBlock.WriteLine();
                                                    goto nextMember;
                                                case "object":
                                                case nameof(Object):
                                                    goto nextMember;
                                                case "bool":
                                                case nameof(Boolean):
                                                    seriMemberBlock.WriteLine($"Unsafe.Write(ptr_current, {memberAccess} ? (byte)1 : (byte)0);");
                                                    seriMemberBlock.WriteLine($"ptr_current = Unsafe.Add<byte>(ptr_current, 1);");
                                                    seriMemberBlock.WriteLine();


                                                    deserMemberBlock.WriteLine($"{memberAccess} = Unsafe.Read<byte>(ptr_current) != 0;");
                                                    deserMemberBlock.WriteLine($"ptr_current = Unsafe.Add<byte>(ptr_current, 1);");
                                                    deserMemberBlock.WriteLine();
                                                    goto nextMember;
                                                case "string":
                                                case nameof(String):

                                                    if (parant_var is null && m.IsConditional && !m.IsArrayRound && !m.IsEnumRound) {
                                                        memberNullables.Add(m.MemberName);
                                                    }
                                                    seriMemberBlock.WriteLine($"CommonCode.WriteString(ref ptr_current, {memberAccess});");
                                                    seriMemberBlock.WriteLine();


                                                    deserMemberBlock.WriteLine($"{memberAccess} = CommonCode.ReadString(ref ptr_current);");
                                                    deserMemberBlock.WriteLine();
                                                    goto nextMember;
                                                default:

                                                    List<(string memberName, string memberValue)> externalMemberValues = GetExternalDefaultValue(m, memberTypeSym);
                                                    string externalMemberValueArgs = "";
                                                    foreach (var (memberName, memberValue) in externalMemberValues) {
                                                        externalMemberValueArgs += $", _{memberName}: {memberValue}";
                                                    }
                                                    if (mType is ArrayTypeSyntax arr) {

                                                        var eleSym = ((IArrayTypeSymbol)memberTypeSym).ElementType;

                                                        if (parant_var is null && m.IsConditional && !m.IsArrayRound && !m.IsEnumRound) {
                                                            memberNullables.Add(m.MemberName);
                                                        }

                                                        if (arr.RankSpecifiers.Count != 1 || m.IsArrayRound) {
                                                            throw new DiagnosticException(
                                                                Diagnostic.Create(
                                                                    new DiagnosticDescriptor(
                                                                        "SCG23",
                                                                        "Array element should not be array",
                                                                        "in netpacket '{0}', element of array '{1}' is a new array mType",
                                                                        "",
                                                                        DiagnosticSeverity.Error,
                                                                        true),
                                                                    m.MemberDeclaration.GetLocation(),
                                                                    packet.TypeName,
                                                                    m.MemberName));
                                                        }

                                                        var arrAtt = m.Attributes.FirstOrDefault(a => a.AttributeMatch<ArraySizeAttribute>()) ??
                                                        throw new DiagnosticException(
                                                            Diagnostic.Create(
                                                                new DiagnosticDescriptor(
                                                                    "SCG24",
                                                                    "Array size attribute missing",
                                                                    "Array mType memberAccesses '{0}' of netpacket '{1}' missing size introduction",
                                                                    "",
                                                                    DiagnosticSeverity.Error,
                                                                    true),
                                                                m.MemberDeclaration.GetLocation(),
                                                                m.MemberName,
                                                                packet.TypeName));

                                                        var indexExps = arrAtt.ExtractAttributeParams();
                                                        if (indexExps.Length != arr.RankSpecifiers[0].Rank) {
                                                            throw new DiagnosticException(
                                                                Diagnostic.Create(
                                                                    new DiagnosticDescriptor(
                                                                        "SCG25",
                                                                        "Array rank conflict",
                                                                        "rank of array size attribute is not match with the real array '{0}' at netpacket '{1}'",
                                                                        "",
                                                                        DiagnosticSeverity.Error,
                                                                        true),
                                                                    m.MemberDeclaration.GetLocation(),
                                                                    m.MemberName,
                                                                    packet.TypeName));
                                                        }
                                                        bool elementRepeating = eleSym.AllInterfaces.Any(i => i.Name == nameof(IRepeatElement<int>));
                                                        if (elementRepeating && arr.RankSpecifiers[0].Rank != 1) {
                                                            throw new DiagnosticException(
                                                                Diagnostic.Create(
                                                                    new DiagnosticDescriptor(
                                                                        "SCG26",
                                                                        "Array rank conflict",
                                                                        $"Arrays that implement '{nameof(IRepeatElement<int>)}' must be one-dimensional",
                                                                        "",
                                                                        DiagnosticSeverity.Error,
                                                                        true),
                                                                    m.MemberDeclaration.GetLocation()));
                                                        }
                                                        object GetArraySize(ExpressionSyntax indexExp, int i) {
                                                            object? size = null;
                                                            if (indexExp is LiteralExpressionSyntax lit) {
                                                                var text = lit.Token.Text;
                                                                if (text.StartsWith("\"") && text.EndsWith("\"")) {
                                                                    size = (parant_var is null ? "" : $"{parant_var}.") + text.Substring(1, text.Length - 2);
                                                                }
                                                                else if (ushort.TryParse(text, out var numSize)) {
                                                                    size = numSize;
                                                                }
                                                            }
                                                            else if (indexExp is InvocationExpressionSyntax inv) {
                                                                if (inv.Expression.ToString() == "nameof") {
                                                                    size = (parant_var is null ? "" : $"{parant_var}.") + inv.ArgumentList.Arguments.First().Expression;
                                                                }
                                                            }
                                                            if (size == null) {
                                                                throw new DiagnosticException(
                                                                    Diagnostic.Create(
                                                                        new DiagnosticDescriptor(
                                                                            "SCG27",
                                                                            "Array rank size invaild",
                                                                            "given size of array rank is invaild, index '{0}' of '{1}' at netpacket '{2}'",
                                                                            "",
                                                                            DiagnosticSeverity.Error,
                                                                            true),
                                                                        m.MemberDeclaration.GetLocation(),
                                                                        i,
                                                                        m.MemberName,
                                                                        packet.TypeName));
                                                            }
                                                            return size;
                                                        }

                                                        if (elementRepeating) {
                                                            indexID += 1;
                                                            var oldMemberNodeCount = deserMemberBlock.Sources.Count;
                                                            seriMemberBlock.Write($"for (int _g_index_{indexID} = 0; _g_index_{indexID} < {memberAccess}.Length; _g_index_{indexID}++) ");
                                                            (seriMemberBlock, deserMemberBlock).BlockWrite((seriMemberBlock, deserMemberBlock) => {
                                                                m.EnterArrayRound(new string[] { $"_g_index_{indexID}" });
                                                                UnfoldMembers(seriMemberBlock, deserMemberBlock, typeSym, new (MemberDataAccessRound, string?)[] { (m, parant_var) });
                                                                m.ExitArrayRound();
                                                            });
                                                            deserMemberBlock.Sources.RemoveRange(oldMemberNodeCount, deserMemberBlock.Sources.Count);
                                                            seriMemberBlock.WriteLine();


                                                            deserMemberBlock.WriteLine($"var _g_elementCount_{indexID} = {GetArraySize(indexExps.First(), 0)};");
                                                            deserMemberBlock.WriteLine($"var _g_arrayCache_{indexID} = ArrayPool<{eleSym.Name}>.Shared.Rent(_g_elementCount_{indexID});");
                                                            deserMemberBlock.WriteLine($"var _g_arrayIndex_{indexID} = 0;");
                                                            deserMemberBlock.Write($"while(_g_elementCount_{indexID} > 0) ");
                                                            deserMemberBlock.BlockWrite((source) => {
                                                                if (eleSym.IsValueType) {
                                                                    source.WriteLine($"_g_arrayCache_{indexID}[_g_arrayIndex_{indexID}] = default;");
                                                                    foreach (var m in externalMembers) {
                                                                        source.WriteLine($"_g_arrayCache_{indexID}[_g_arrayIndex_{indexID}].{m.memberName} = _{m.memberName};");
                                                                    }
                                                                    source.WriteLine($"_g_arrayCache_{indexID}[_g_arrayIndex_{indexID}].ReadContent(ref ptr_current);");
                                                                }
                                                                else {
                                                                    source.WriteLine($"_g_arrayCache_{indexID}[_g_arrayIndex_{indexID}] = new (ref ptr_current);");
                                                                }
                                                                source.WriteLine($"_g_elementCount_{indexID} -= _g_arrayCache_{indexID}[_g_arrayIndex_{indexID}].{nameof(IRepeatElement<int>.RepeatCount)} + 1;");
                                                                source.WriteLine($"++_g_arrayIndex_{indexID};");
                                                            });
                                                            deserMemberBlock.WriteLine();
                                                            deserMemberBlock.WriteLine($"{memberAccess} = new {eleSym.Name}[_g_arrayIndex_{indexID}];");
                                                            deserMemberBlock.WriteLine($"Array.Copy(_g_arrayCache_{indexID}, {memberAccess}, _g_arrayIndex_{indexID});");
                                                            deserMemberBlock.WriteLine($"ArrayPool<{eleSym.Name}>.Shared.Return(_g_arrayCache_{indexID});");
                                                            deserMemberBlock.WriteLine();
                                                            goto nextMember;
                                                        }
                                                        else {
                                                            string[] indexNames = new string[indexExps.Length];
                                                            object[] rankSizes = new object[indexExps.Length];
                                                            for (int i = 0; i < indexNames.Length; i++) {
                                                                indexID += 1;
                                                                indexNames[i] = $"_g_index_{indexID}";
                                                                rankSizes[i] = GetArraySize(indexExps[i], i);
                                                            }
                                                            (BlockNode writeArrayBlock, BlockNode readArrayBlock) arrayGroup = default;
                                                            int index = indexExps.Length - 1;
                                                            do {
                                                                var indexName = indexNames[index];
                                                                var indexExp = indexExps[index];
                                                                var head = $"for (int {indexName} = 0; {indexName} < {rankSizes[index]}; {indexName}++) ";
                                                                if (arrayGroup == default) {
                                                                    arrayGroup = (new BlockNode(seriMemberBlock), new BlockNode(deserMemberBlock));
                                                                    m.EnterArrayRound(indexNames);
                                                                    UnfoldMembers(arrayGroup.writeArrayBlock, arrayGroup.readArrayBlock, typeSym, new (MemberDataAccessRound, string?)[] { (m, parant_var) });
                                                                    m.ExitArrayRound();
                                                                }
                                                                arrayGroup.readArrayBlock?.WarpBlock((head, false));
                                                                arrayGroup.writeArrayBlock?.WarpBlock((head, false));
                                                            }
                                                            while (index-- > 0);
                                                            seriMemberBlock.Sources.AddRange(arrayGroup.writeArrayBlock.Sources);

                                                            deserMemberBlock.WriteLine($"{memberAccess} = new {arr.ElementType}[{string.Join(", ", rankSizes)}];");
                                                            deserMemberBlock.Sources.AddRange(arrayGroup.readArrayBlock.Sources);
                                                        }
                                                        seriMemberBlock.WriteLine();
                                                        goto nextMember;
                                                    }

                                                    if (parant_var is null && m.IsConditional && memberTypeSym.IsReferenceType && !m.IsArrayRound && !m.IsEnumRound) {
                                                        memberNullables.Add(m.MemberName);
                                                    }


                                                    if (memberTypeSym.IsAbstract) {
                                                        var mTypefullName = memberTypeSym.GetFullName();
                                                        if (abstractTypesSymbols.Any(abs => abs.GetFullName() == mTypefullName)) {
                                                            deserMemberBlock.WriteLine($"{memberAccess} = {memberTypeSym.Name}.Read{memberTypeSym.Name}(ref ptr_current{externalMemberValueArgs});");
                                                        }
                                                        else {
                                                            throw new DiagnosticException(
                                                                Diagnostic.Create(
                                                                    new DiagnosticDescriptor(
                                                                        "SCG28",
                                                                        "unexcepted abstract member type",
                                                                        "abstract type '{0}' of packet '{1}' member '{2}' should defined in current assembly.",
                                                                        "",
                                                                        DiagnosticSeverity.Error,
                                                                        true),
                                                                    m.MemberType.GetLocation(),
                                                                    memberTypeSym.Name,
                                                                    typeSym.Name,
                                                                    m.MemberName));
                                                        }
                                                    }

                                                    if (memberTypeSym.AllInterfaces.Any(i => i.Name == nameof(ISoildSerializableData))) {
                                                        if (!memberTypeSym.IsUnmanagedType) {
                                                            throw new DiagnosticException(
                                                                Diagnostic.Create(
                                                                    new DiagnosticDescriptor(
                                                                        "SCG29",
                                                                        "unexcepted type definition",
                                                                        "Only unmanaged types can implement this interface.",
                                                                        "",
                                                                        DiagnosticSeverity.Error,
                                                                        true),
                                                                    m.MemberType.GetLocation()));
                                                        }
                                                        seriMemberBlock.WriteLine($"Unsafe.Write(ptr_current, {memberAccess});");
                                                        seriMemberBlock.WriteLine($"ptr_current = Unsafe.Add<{mTypeStr}>(ptr_current, 1);");
                                                        seriMemberBlock.WriteLine();

                                                        if (!memberTypeSym.IsAbstract) {
                                                            deserMemberBlock.WriteLine($"{memberAccess} = Unsafe.Read<{mTypeStr}>(ptr_current);");
                                                            deserMemberBlock.WriteLine($"ptr_current = Unsafe.Add<{mTypeStr}>(ptr_current, 1);");
                                                            deserMemberBlock.WriteLine();
                                                        }
                                                        goto nextMember;
                                                    }

                                                    if (memberTypeSym.AllInterfaces.Any(i => i.Name == nameof(ISerializableData))) {

                                                        if (externalMemberValues.Count > 0) {
                                                            if (memberTypeSym.IsUnmanagedType) {
                                                                seriMemberBlock.WriteLine($"var _temp_{m.MemberName} = {memberAccess};");
                                                                foreach (var (memberName, memberValue) in externalMemberValues) {
                                                                    seriMemberBlock.WriteLine($"_temp_{m.MemberName}.{memberName} = {memberValue};");
                                                                }
                                                                seriMemberBlock.WriteLine($"{memberAccess} = _temp_{m.MemberName};");
                                                            }
                                                            else {
                                                                foreach (var (memberName, memberValue) in externalMemberValues) {
                                                                    seriMemberBlock.WriteLine($"{memberAccess}.{memberName} = {memberValue};");
                                                                }
                                                            }
                                                        }

                                                        seriMemberBlock.WriteLine($"{memberAccess}.WriteContent(ref ptr_current);");
                                                        seriMemberBlock.WriteLine();


                                                        if (!memberTypeSym.IsAbstract) {
                                                            if (memberTypeSym.AllInterfaces.Any(t => t.Name == nameof(ILengthDependent))) {

                                                                if (!typeSym.AllInterfaces.Any(t => t.Name == nameof(ILengthDependent))) {
                                                                    throw new DiagnosticException(
                                                                        Diagnostic.Create(
                                                                            new DiagnosticDescriptor(
                                                                                "SCG30",
                                                                                "unexcepted member type",
                                                                                $"Members that implement '{nameof(ILengthDependent)}' must be defined within a type that also implements '{nameof(ILengthDependent)}'.",
                                                                                "",
                                                                                DiagnosticSeverity.Error,
                                                                                true),
                                                                            m.MemberType.GetLocation()));
                                                                }

                                                                if (memberTypeSym.IsUnmanagedType) {
                                                                    if(externalMembers.Count > 0) {
                                                                        var variableName = $"_temp_{m.MemberName}";
                                                                        deserMemberBlock.WriteLine($"var {variableName} = {memberAccess};");
                                                                        foreach (var m2 in externalMembers) {
                                                                            source.WriteLine($"{variableName}.{m2.memberName} = _{m2.memberName};");
                                                                        }
                                                                        deserMemberBlock.WriteLine($"{memberAccess} = {variableName};");
                                                                    }
                                                                    deserMemberBlock.WriteLine($"{memberAccess}.ReadContent(ref ptr_current, ptr_end);");
                                                                }
                                                                else {
                                                                    deserMemberBlock.WriteLine($"{memberAccess} = new (ref ptr_current, ptr_end{externalMemberValueArgs});");
                                                                }
                                                            }
                                                            else {
                                                                if (memberTypeSym.IsUnmanagedType) {
                                                                    if (externalMembers.Count > 0) {
                                                                        var variableName = $"_temp_{m.MemberName}";
                                                                        deserMemberBlock.WriteLine($"var {variableName} = {memberAccess};");
                                                                        foreach (var m2 in externalMembers) {
                                                                            source.WriteLine($"{variableName}.{m2.memberName} = _{m2.memberName};");
                                                                        }
                                                                        deserMemberBlock.WriteLine($"{memberAccess} = {variableName};");
                                                                    }
                                                                    deserMemberBlock.WriteLine($"{memberAccess}.ReadContent(ref ptr_current);");
                                                                }
                                                                else {
                                                                    deserMemberBlock.WriteLine($"{memberAccess} = new (ref ptr_current{externalMemberValueArgs});");
                                                                }
                                                            }
                                                            deserMemberBlock.WriteLine();
                                                        }
                                                        goto nextMember;
                                                    }

                                                    var seqType = memberTypeSym.AllInterfaces.FirstOrDefault(i => i.Name == nameof(ISequentialSerializableData<byte>))?.TypeArguments.First();
                                                    if (seqType != null) {
                                                        seriMemberBlock.WriteLine($"Unsafe.Write(ptr_current, {memberAccess}.{nameof(ISequentialSerializableData<byte>.SequentialData)});");
                                                        seriMemberBlock.WriteLine($"ptr_current = Unsafe.Add<{seqType.GetFullTypeName()}>(ptr_current, 1);");
                                                        seriMemberBlock.WriteLine();


                                                        var seqTypeName = seqType.GetFullTypeName();
                                                        if (m.IsProperty) {
                                                            var varName = $"gen_var_{parant_var}_{m.MemberName}";
                                                            deserMemberBlock.WriteLine($"{seqTypeName} {varName} = {(seqType.IsUnmanagedType ? "default" : "new ()")};");

                                                            deserMemberBlock.WriteLine($"{varName}.{nameof(ISequentialSerializableData<byte>.SequentialData)} = Unsafe.Read<{seqTypeName}>(ptr_current);");
                                                            deserMemberBlock.WriteLine($"{memberAccess} = {varName};");
                                                        }
                                                        else {
                                                            deserMemberBlock.WriteLine($"{memberAccess}.{nameof(ISequentialSerializableData<byte>.SequentialData)} = Unsafe.Read<{seqTypeName}>(ptr_current);");
                                                        }
                                                        deserMemberBlock.WriteLine($"ptr_current = Unsafe.Add<{seqTypeName}>(ptr_current, 1);");
                                                        deserMemberBlock.WriteLine();
                                                        goto nextMember;
                                                    }
                                                    else if (memberTypeSym is INamedTypeSymbol { EnumUnderlyingType: not null } namedEnumTypeSym) {
                                                        m.EnterEnumRound((memberTypeSym, namedEnumTypeSym.EnumUnderlyingType));
                                                        UnfoldMembers(seriMemberBlock, deserMemberBlock, typeSym, new (MemberDataAccessRound, string?)[] { (m, parant_var) });
                                                        m.ExitEnumRound();
                                                        goto nextMember;
                                                    }
                                                    else {
                                                        if (memberTypeSym is INamedTypeSymbol namedSym) {

                                                            if (Compilation.TryGetTypeDefSyntax(mTypeStr, out var tdef, fullNamespace, usings) && tdef is not null) {
                                                                var varName = $"gen_var_{parant_var}_{m.MemberName}";
                                                                seriMemberBlock.WriteLine($"{mTypeStr} {varName} = {memberAccess};");

                                                                deserMemberBlock.WriteLine($"{mTypeStr} {varName} = {(namedSym.IsUnmanagedType ? "default" : "new ()")};");

                                                                foreach (var (memberName, memberValue) in externalMemberValues) {
                                                                    seriMemberBlock.WriteLine($"{varName}.{memberName} = _{memberName};");
                                                                }

                                                                UnfoldMembers(seriMemberBlock, deserMemberBlock, namedSym, Transform(tdef).Members.Select<MemberDataAccessRound, (MemberDataAccessRound, string?)>(m => (m, varName)));

                                                                deserMemberBlock.WriteLine($"{memberAccess} = {varName};");
                                                                goto nextMember;
                                                            }
                                                        }
                                                        throw new DiagnosticException(
                                                            Diagnostic.Create(
                                                                new DiagnosticDescriptor(
                                                                    "SCG31",
                                                                    "unexcepted member type",
                                                                    "Generating an inline member '{0}' serialization method of {1} encountered a member type {2} that could not generate a resolution.",
                                                                    "",
                                                                    DiagnosticSeverity.Error,
                                                                    true),
                                                                m.MemberType.GetLocation(),
                                                                m.MemberName,
                                                                typeSym.Name,
                                                                memberTypeSym));
                                                    }
                                            }

                                        nextMember:
                                            MemberConditionCheck(typeSym, m, memberTypeSym, parant_var, seriMemberBlock, true);
                                            seriNode.Sources.AddRange(seriMemberBlock.Sources);
                                            MemberConditionCheck(typeSym, m, memberTypeSym, parant_var, deserMemberBlock, false);
                                            deserNode.Sources.AddRange(deserMemberBlock.Sources);
                                        }
                                    }
                                    catch (DiagnosticException de) {
                                        context.ReportDiagnostic(de.Diagnostic);
                                        return;
                                    }
                                }
                                #endregion

                                #region WriteContent

                                var writeNode = new BlockNode(classNode);
                                var readNode = new BlockNode(classNode);

                                writeNode.WriteLine("var ptr_current = ptr;");
                                writeNode.WriteLine();
                                if (packet.IsSubmodel) {
                                    Stack<(string idtype, string idProp)> inherits = new();
                                    var find = typeSym.BaseType;
                                    while (find is not null && find.IsAbstract) {
                                        var att = find.GetAttributes().FirstOrDefault(a => a.AttributeClass?.Name == nameof(AbstractModelAttribute));
                                        if (att is not null) {
                                            inherits.Push(
                                                (
                                                ((INamedTypeSymbol?)att.ConstructorArguments[0].Value)?.EnumUnderlyingType?.GetPredifinedName() ?? throw new NullReferenceException(),
                                                (string)(att.ConstructorArguments[1].Value ?? throw new NullReferenceException())
                                                ));
                                            find = find.BaseType;
                                        }
                                    }
                                    while (inherits.Count > 0) {
                                        (string idtype, string idProp) = inherits.Pop();
                                        writeNode.WriteLine($"Unsafe.Write(ptr_current, ({idtype}){idProp});");
                                        writeNode.WriteLine($"ptr_current = Unsafe.Add<{idtype}>(ptr_current, 1);");
                                        writeNode.WriteLine();
                                    }
                                }


                                readNode.WriteLine();

                                if (packet.CompressData != default) {
                                    writeNode.WriteLine($"var ptr_compressed = ptr_current;");
                                    writeNode.WriteLine($"var rawBuffer = ArrayPool<byte>.Shared.Rent({packet.CompressData.bufferSize});");
                                    writeNode.Write("fixed (void* ptr_rawdata = rawBuffer)");

                                    readNode.WriteLine($"var decompressedBuffer = ArrayPool<byte>.Shared.Rent({packet.CompressData.bufferSize});");
                                    readNode.Write("fixed (void* ptr_decompressed = decompressedBuffer)");

                                    (writeNode, readNode).BlockWrite((writeNode, readNode) => {
                                        writeNode.WriteLine("ptr_current = ptr_rawdata;");

                                        readNode.WriteLine("var ptr_current = ptr_decompressed;");
                                        readNode.WriteLine($"CommonCode.ReadDecompressedData(ptr, ref ptr_current, (int)((long)ptr_end - (long)ptr));");
                                        readNode.WriteLine("ptr_current = ptr_decompressed;");

                                        UnfoldMembers(writeNode, readNode, typeSym, packet.Members.Select<MemberDataAccessRound, (MemberDataAccessRound, string?)>(m => (m, null)));
                                        writeNode.WriteLine($"CommonCode.WriteCompressedData(ptr_rawdata, ref ptr_compressed, (int)((long)ptr_current - (long)ptr_rawdata), {packet.CompressData.compressLevel});");
                                        writeNode.WriteLine($"ArrayPool<byte>.Shared.Return(rawBuffer);");
                                        writeNode.WriteLine("ptr = ptr_compressed;");
                                    });
                                    readNode.WriteLine("ptr = ptr_end;");
                                    readNode.WriteLine($"ArrayPool<byte>.Shared.Return(decompressedBuffer);");
                                }
                                else {
                                    readNode.WriteLine("var ptr_current = ptr;");

                                    UnfoldMembers(writeNode, readNode, typeSym, packet.Members.Select<MemberDataAccessRound, (MemberDataAccessRound, string?)>(m => (m, null)));
                                    writeNode.WriteLine("ptr = ptr_current;");
                                    readNode.WriteLine();
                                    if (packet.HasExtraData) {
                                        writeNode.WriteLine($"Marshal.Copy({nameof(IExtraData.ExtraData)}, 0, (IntPtr)ptr_current, {nameof(IExtraData.ExtraData)}.Length);");
                                        writeNode.WriteLine($"ptr_current = Unsafe.Add<byte>(ptr_current, {nameof(IExtraData.ExtraData)}.Length);");

                                        readNode.WriteLine("var restContentSize = (int)((long)ptr_end - (long)ptr_current);");
                                        readNode.WriteLine($"{nameof(IExtraData.ExtraData)} = new byte[restContentSize];");
                                        readNode.WriteLine($"Marshal.Copy((IntPtr)ptr_current, {nameof(IExtraData.ExtraData)}, 0, restContentSize);");
                                        readNode.WriteLine("ptr = ptr_end;");
                                    }
                                    else {
                                        readNode.WriteLine("ptr = ptr_current;");
                                    }
                                }

                                if (!packet.TypeDeclaration.Members.Any(m => {
                                    if (m is MethodDeclarationSyntax method && method.Identifier.Text == "WriteContent") {
                                        var param = method.ParameterList.Parameters;
                                        if (
                                        param.Count == 1 &&
                                        param[0].Type is PointerTypeSyntax pointerType &&
                                        pointerType.ElementType.ToString() is "void" &&
                                        param[0].Modifiers.Count == 1 &&
                                        param[0].Modifiers[0].ToString() is "ref") {
                                            return true;
                                        }
                                    }
                                    return false;
                                })) {
                                    classNode.Write($"public unsafe {(packet.IsSubmodel ? "override " : "")}void WriteContent(ref void* ptr) ");
                                    classNode.Sources.Add(writeNode);
                                }
                                #endregion

                                #region ReadContent

                                if (!packet.TypeDeclaration.Members.Any(m => {
                                    if (m is MethodDeclarationSyntax method && method.Identifier.Text == "ReadContent") {
                                        var param = method.ParameterList.Parameters;
                                        if (
                                        param.Count == 1 &&
                                        param[0].Type is PointerTypeSyntax pointerType &&
                                        pointerType.ElementType.ToString() is "void" &&
                                        param[0].Modifiers.Count == 1 &&
                                        param[0].Modifiers[0].ToString() is "ref") {
                                            return true;
                                        }
                                    }
                                    return false;
                                })) {

                                    if (packet.LengthDependent) {

                                        classNode.WriteLine("/// <summary>");
                                        classNode.WriteLine("/// This operation is not supported and always throws a System.NotSupportedException.");
                                        classNode.WriteLine("/// </summary>");
                                        classNode.WriteLine($"[Obsolete]");
                                        if (packet.IsSubmodel) {
                                            classNode.WriteLine($"public override void ReadContent(ref void* ptr) => throw new {nameof(NotSupportedException)}();");
                                        }
                                        else {
                                            classNode.WriteLine($"void {nameof(ISerializableData)}.ReadContent(ref void* ptr) => throw new {nameof(NotSupportedException)}();");
                                        }
                                        classNode.WriteLine($"[MemberNotNull({string.Join(", ", memberNullables.Select(m => $"nameof({m})"))})]");
                                        classNode.Write($"public unsafe void ReadContent(ref void* ptr, void* ptr_end) ");
                                    }
                                    else {
                                        classNode.WriteLine($"[MemberNotNull({string.Join(", ", memberNullables.Select(m => $"nameof({m})"))})]");
                                        classNode.Write($"public unsafe {(packet.IsSubmodel ? "override " : "")}void ReadContent(ref void* ptr) ");
                                    }
                                    classNode.Sources.Add(readNode);
                                }
                                #endregion
                            });
                        });

                        #region Write using
                        foreach (var us in usings.Concat(NeccessaryUsings).Distinct()) {

                            usingTarget.NewLineAfter($"using {us};");
                        }
                        #endregion

                        context.AddSource($"{typeSym.GetFullName()}.seri.g.cs", SourceText.From(file.ToString(), Encoding.UTF8));
                    }
                    else {
                        throw new DiagnosticException(
                            Diagnostic.Create(
                                new DiagnosticDescriptor(
                                    "SCG32",
                                    "unexcepted type definition missing",
                                    "The type '{0}' cannot be found in compilation",
                                    "",
                                    DiagnosticSeverity.Error,
                                    true),
                                packet.TypeDeclaration.GetLocation(),
                                packet.TypeName));
                    }
                }
                catch (DiagnosticException de) {
                    context.ReportDiagnostic(de.Diagnostic);
                    continue;
                }
            }
            #endregion

            #region Foreach abstract class and add static deseriailize method
            foreach (var (packet, enumType, enumProp, list) in knownPackets.Values) {

                if (packet is not null) {
                    packet.TypeDeclaration.GetNamespace(out var classes, out var fullNamespace, out var unit);
                    var usings = unit?.Usings.Select(u => u.Name.ToString()).ToArray() ?? Array.Empty<string>();

                    var source = new SourceCodeWriter(1024 * 4);
                    source.WriteLine();
                    source.WriteLine("// <auto-generated>");
                    source.WriteLine();

                    foreach (var us in usings.Concat(NeccessaryUsings).Concat(list.Select(l => {
                        l.packet.TypeDeclaration.GetNamespace(out _, out var ns, out _);
                        return ns;
                    })).Distinct()) {

                        source.WriteLine($"using {us};");
                    }


                    List<(string memberName, string memberType)> externalMembers = packet.TypeDeclaration.Members
                        .Where(m => m.AttributeLists
                        .SelectMany(a => a.Attributes)
                        .Any(a => a.AttributeMatch<ExternalMemberAttribute>()))
                        .Select((m) => {
                            if (m is PropertyDeclarationSyntax prop) {
                                return new List<(string, string)>() { (prop.Identifier.ToString(), prop.Type.ToString()) };
                            }
                            else {
                                var field = (FieldDeclarationSyntax)m;
                                var list = new List<(string memberName, string memberType)>();
                                foreach (var variable in field.Declaration.Variables) {
                                    list.Add((variable.Identifier.ToString(), field.Declaration.Type.ToString()));
                                }
                                return list;
                            }
                        }).SelectMany(m => m).ToList();
                    string externalMemberParams;
                    string externalMemberParamsCall;
                    if (externalMembers.Count == 0) {
                        externalMemberParams = "";
                        externalMemberParamsCall = "";
                    }
                    else {
                        externalMemberParams = $", {string.Join(", ", externalMembers.Select(m => $"{m.memberType} _{m.memberName} = default"))}";
                        externalMemberParamsCall = $", {string.Join(", ", externalMembers.Select(m => $"_{m.memberName}"))}";
                    }
                    source.WriteLine();
                    source.Write($"namespace {fullNamespace} ");
                    source.BlockWrite((source) => {
                        source.Write($"public unsafe partial class {packet.TypeName} ");
                        source.BlockWrite((source) => {
                            source.Write($"public unsafe static {packet.TypeName} Read{packet.TypeName}(ref void* ptr{(packet.IsNetPacket ? ", void* ptr_end" : "")}{(packet.IsNetPacket ? ", bool isServerSide" : "")}{externalMemberParams}) ");
                            source.BlockWrite((source) => {
                                source.WriteLine($"{enumType.Name} identity = ({enumType.Name})Unsafe.Read<{enumType.EnumUnderlyingType}>(ptr);");
                                source.WriteLine($"ptr = Unsafe.Add<{enumType.EnumUnderlyingType}>(ptr, 1);");
                                source.Write($"switch (identity) ");
                                source.BlockWrite((source) => {
                                    foreach (var enumValue in enumType.GetMembers().OfType<IFieldSymbol>()) {
                                        var packetMatch = list.FirstOrDefault(a => a.enumMemberName == enumValue.Name);
                                        if (packetMatch.packet is not null) {
                                            if (packetMatch.packet.IsAbstract) {
                                                source.WriteLine($"case {enumType.Name}.{enumValue.Name}: return {packetMatch.packet.TypeName}.Read{packetMatch.packet.TypeName}(ref ptr{(packet.IsNetPacket ? ", ptr_end" : "")}{(packet.IsNetPacket ? ", isServerSide" : "")}{externalMemberParamsCall});");
                                            }
                                            else {
                                                source.WriteLine($"case {enumType.Name}.{enumValue.Name}: return new {packetMatch.packet.TypeName}(ref ptr{(packetMatch.packet.LengthDependent ? ", ptr_end" : "")}{(packetMatch.packet.SideDependent ? ", isServerSide" : "")}{externalMemberParamsCall});");
                                            }
                                        }
                                    }
                                    source.WriteLine($"default: throw new {nameof(UnsupportSubmodelTypeException)}(typeof({packet.TypeName}), identity, (long)identity);");
                                });
                            });
                        });
                    });
                    context.AddSource($"{packet.TypeDeclaration.GetFullName()}.static.seri.g.cs", SourceText.From(source.ToString(), Encoding.UTF8));
                }
            }
            #endregion
        }

        static CompilationMonitor Compilation = new CompilationMonitor();

        public void Initialize(IncrementalGeneratorInitializationContext initContext) {

#if DEBUG
            //if (!Debugger.IsAttached) {
            //    Debugger.Launch();
            //}
#endif

            initContext.RegisterSourceOutput(initContext.CompilationProvider.WithComparer(Compilation), Compilation.LoadCompilation);


            var classes = initContext.SyntaxProvider.CreateSyntaxProvider(predicate: FilterTypes, transform: Transform).Collect();
            var combine = initContext.CompilationProvider.Combine(classes);
            initContext.RegisterSourceOutput(combine, Execute);
        }
    }
}