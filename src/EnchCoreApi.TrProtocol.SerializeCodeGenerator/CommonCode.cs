using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics.CodeAnalysis;

namespace EnchCoreApi.TrProtocol.SerializeCodeGenerator {
    public static class CommonCode {
        public static void GetNamespace(this TypeDeclarationSyntax type, out string[] typeParentsNames, out string? fullNameSpce, out CompilationUnitSyntax? compilationUnit) {
            var parent = type.Parent;
            List<string> names = new List<string>() {
                type.Identifier.Text,
            };
            while (parent is ClassDeclarationSyntax classParent) {
                names.Insert(0, classParent.Identifier.Text);
                parent = classParent.Parent;
            }
            typeParentsNames = names.ToArray();
            fullNameSpce = null;
            while (parent is NamespaceDeclarationSyntax @namespace) {
                if (fullNameSpce is null) {
                    fullNameSpce = @namespace.Name.ToString();
                }
                else {
                    fullNameSpce = @namespace.Name.ToString() + "." + fullNameSpce;
                }
                parent = @namespace.Parent;
            }
            if (parent is FileScopedNamespaceDeclarationSyntax fileScopedNamespace) {
                fullNameSpce = fileScopedNamespace.Name.ToString();
                parent = fileScopedNamespace.Parent;
            }
            compilationUnit = parent as CompilationUnitSyntax;
        }
        public static string GetFullName(this TypeDeclarationSyntax type) {
            type.GetNamespace(out var typeParentsNames, out var ns, out _);
            if (ns is not null) {
                return string.Join(".", new string[] { ns }.Concat(typeParentsNames)) + "." + type.Identifier;
            }
            return string.Join(".", typeParentsNames) + "." + type.Identifier;
        }
        public static bool GetParent<TNode>(this SyntaxNode node, [NotNullWhen(true)] out TNode? result) where TNode : SyntaxNode {
            SyntaxNode? parent = node;
            result = null;
            do {
                parent = parent.Parent;
                if (parent is TNode n) {
                    result = n;
                    return true;
                }
            }
            while (parent is not null);
            return false;
        }
        public static bool IsLiteralExpression(this ExpressionSyntax expression, [NotNullWhen(true)] out string? text) {
            if (expression is LiteralExpressionSyntax lite) {
                text = lite.Token.Text; return true;
            }
            text = null;
            return false;
        }
        public static bool AttributeMatch<TAttribute>(this MemberDeclarationSyntax member) where TAttribute : Attribute {
            return member.AttributeLists.SelectMany(list => list.Attributes).Any(att => att.AttributeMatch<TAttribute>());
        }
        public static bool AttributeMatch<TAttribute>(this AttributeSyntax attribute) where TAttribute : Attribute {
            var name = typeof(TAttribute).Name;
            var name1 = attribute.Name.ToString();
            var name2 = attribute.Name.ToString() + "Attribute";
            return name == name1 || name == name2;
        }
        public static ExpressionSyntax[] ExtractAttributeParams(this AttributeSyntax attribute) {
            if (attribute.ArgumentList == null) {
                return Array.Empty<ExpressionSyntax>();
            }
            if (attribute.ArgumentList.Arguments.Count == 1 && attribute.ArgumentList.Arguments.First().Expression is InitializerExpressionSyntax init) {
                return init.Expressions.ToArray();
            }
            else {
                return attribute.ArgumentList.Arguments.Select(a => a.Expression).ToArray();
            }
        }
        public static string GetTypeSymbolName(this TypeSyntax type) {
            if (type is PredefinedTypeSyntax predefined) {
                return (predefined.Keyword.Kind()) switch {
                    SyntaxKind.BoolKeyword => nameof(Boolean),
                    SyntaxKind.ByteKeyword => nameof(Byte),
                    SyntaxKind.SByteKeyword => nameof(SByte),
                    SyntaxKind.IntKeyword => nameof(Int32),
                    SyntaxKind.UIntKeyword => nameof(UInt32),
                    SyntaxKind.ShortKeyword => nameof(Int16),
                    SyntaxKind.UShortKeyword => nameof(UInt16),
                    SyntaxKind.LongKeyword => nameof(Int64),
                    SyntaxKind.ULongKeyword => nameof(UInt64),
                    SyntaxKind.FloatKeyword => nameof(Single),
                    SyntaxKind.DoubleKeyword => nameof(Double),
                    SyntaxKind.DecimalKeyword => nameof(Decimal),
                    SyntaxKind.StringKeyword => nameof(String),
                    SyntaxKind.CharKeyword => nameof(Char),
                    SyntaxKind.ObjectKeyword => nameof(Object),
                    SyntaxKind.VoidKeyword => "Void",
                    _ => type.ToString()
                };
            }
            return type.ToString();
        }
        public static string GetPredifinedName(this ITypeSymbol type) {
            return type.Name switch {
                nameof(Boolean) => "bool",
                nameof(Byte) => "byte",
                nameof(SByte) => "sbyte",
                nameof(Int32) => "int",
                nameof(UInt32) => "uint",
                nameof(Int16) => "short",
                nameof(UInt16) => "ushort",
                nameof(Int64) => "long",
                nameof(UInt64) => "ulong",
                nameof(Single) => "float",
                nameof(Double) => "double",
                nameof(Decimal) => "decimal",
                nameof(String) => "string",
                nameof(Char) => "char",
                nameof(Object) => "object",
                "Void" => "void",
                _ => type.Name,
            };
        }
        public static string GetFullTypeName(this ITypeSymbol type)
        {
            var name = type.Name;
            var parent = type.ContainingSymbol;
            while (parent is ITypeSymbol t && !string.IsNullOrEmpty(t.Name))
            {
                name = $"{t.Name}.{name}";
                parent = t.ContainingSymbol;
            }
            return name;
        }
        public static string GetFullNamespace(this ITypeSymbol type)
        {
            if (type.ContainingNamespace is not null)
            {
                var name = type.ContainingNamespace.Name;
                var parent = type.ContainingNamespace.ContainingNamespace;
                while (parent is INamespaceSymbol n && !string.IsNullOrEmpty(n.Name))
                {
                    name = $"{n.Name}.{name}";
                    parent = n.ContainingNamespace;
                }
                return name;
            }
            return "";
        }
        public static string GetFullName(this ITypeSymbol type) {
            var name = type.Name;
            var parent = type.ContainingSymbol;
            while (parent is INamespaceOrTypeSymbol t && !string.IsNullOrEmpty(t.Name)) {
                name = $"{t.Name}.{name}";
                parent = t.ContainingSymbol;
            }
            return name;
        }
        public static bool InheritFrom(this ITypeSymbol type, string parentName) {
            var parent = type.BaseType;
            while (parent is not null) {
                if (parent.Name == parentName) {
                    return true;
                }
                parent = parent.BaseType;
            }
            return false;
        }
        public static bool IsOrInheritFrom(this ITypeSymbol type, string parentName) => type.Name == parentName || type.InheritFrom(parentName);
    }
}