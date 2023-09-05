using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics.CodeAnalysis;

namespace EnchCoreApi.TrProtocol.SerializeCodeGenerator {
    public class CompilationMonitor : IEqualityComparer<Compilation> {
        bool IEqualityComparer<Compilation>.Equals(Compilation x, Compilation y) {
            var ax = x.Assembly;
            var ay = y.Assembly;
            if (ax.NamespaceNames.Count != ay.NamespaceNames.Count) {
                return false;
            }
            using var xns = ax.NamespaceNames.GetEnumerator();
            using var yns = ay.NamespaceNames.GetEnumerator();
            while (xns.MoveNext() && yns.MoveNext()) {
                if (xns.Current != yns.Current) {
                    return false;
                }
            }
            if (ax.TypeNames.Count != ay.TypeNames.Count) {
                return false;
            }
            using var xts = ax.NamespaceNames.GetEnumerator();
            using var yts = ay.NamespaceNames.GetEnumerator();
            while (xts.MoveNext() && yts.MoveNext()) {
                if (xts.Current != yts.Current) {
                    return false;
                }
            }
            if (x.SourceModule.ReferencedAssemblySymbols.Count() != y.SourceModule.ReferencedAssemblySymbols.Count()) {
                return false;
            }
            for (int i = 0; i < x.SourceModule.ReferencedAssemblySymbols.Length; i++) {
                if (!x.SourceModule.ReferencedAssemblySymbols[i].Equals(y.SourceModule.ReferencedAssemblySymbols[i], SymbolEqualityComparer.Default)) {
                    return false;
                }
            }
            return true;
        }

        int IEqualityComparer<Compilation>.GetHashCode(Compilation obj) => obj.GetHashCode();


        public void LoadCompilation(SourceProductionContext arg1, Compilation arg2) {
            LoadCompilation(arg2);
        }
        public void LoadCompilation(Compilation compilation) {
            LoadTypeSymbol(compilation.Assembly, compilation.SourceModule.ReferencedAssemblySymbols);
            LoadTypeSyntax(compilation.SyntaxTrees);
        }


        Dictionary<string, List<INamedTypeSymbol>> CacheTypeSymbol = new Dictionary<string, List<INamedTypeSymbol>>(4000);
        Dictionary<string, List<INamedTypeSymbol>> LocalCacheTypeSymbol = new Dictionary<string, List<INamedTypeSymbol>>(100);
        private void LoadTypeSymbol(IAssemblySymbol local, IEnumerable<IAssemblySymbol> references) {
            CacheTypeSymbol.Clear();
            LocalCacheTypeSymbol.Clear();
            references = references.Concat(new IAssemblySymbol[] { local });

            static IEnumerable<(INamespaceSymbol nameSpace, string fullName)> GetAllNamespace(IAssemblySymbol assembly) {
                static IEnumerable<(INamespaceSymbol, string)> Get(INamespaceSymbol ns, string? parentName) {
                    var lastName = parentName == null ? ns.Name : $"{parentName}.{ns.Name}";
                    yield return (ns, lastName);
                    foreach (var n in ns.GetMembers().OfType<INamespaceSymbol>()) {
                        foreach (var n2 in Get(n, lastName)) {
                            yield return n2;
                        }
                    }
                }
                foreach (var ns in assembly.GlobalNamespace.GetMembers().OfType<INamespaceSymbol>()) {
                    foreach (var n in Get(ns, null)) {
                        yield return n;
                    }
                }
            }

            foreach (var refAssembly in references) {
                foreach (var nameSpace in GetAllNamespace(refAssembly)) {
                    foreach (var type in nameSpace.nameSpace.GetTypeMembers()) {
                        if (CacheTypeSymbol.TryGetValue(type.Name, out var types)) {
                            types.Add(type);
                        }
                        else {
                            CacheTypeSymbol.Add(type.Name, new List<INamedTypeSymbol>() { type });
                        }
                    }
                }
            }
            foreach (var nameSpace in GetAllNamespace(local)) {
                foreach (var type in nameSpace.nameSpace.GetTypeMembers()) {
                    if (LocalCacheTypeSymbol.TryGetValue(type.Name, out var types)) {
                        types.Add(type);
                    }
                    else {
                        LocalCacheTypeSymbol.Add(type.Name, new List<INamedTypeSymbol>() { type });
                    }
                }
            }
        }
        public IEnumerable<ITypeSymbol> GetLocalTypesSymbol() {
            return LocalCacheTypeSymbol.Values.SelectMany(list => list).ToArray();
        }
        public bool TryGetTypeSymbol(string name, [NotNullWhen(true)] out INamedTypeSymbol? type, string? currentNameSpace, string[] usings) {
            type = null;
            if (CacheTypeSymbol.TryGetValue(name, out var types)) {
                if (types.Count == 0) return false;
                if (types.Count == 1) {
                    type = types[0];
                    return true;
                }
                else {
                    foreach (var u in currentNameSpace is null ? usings : usings.Concat(new string[] { currentNameSpace })) {
                        foreach (var t in types) {
                            if (t.GetFullName() == u + "." + name) {
                                type = t;
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }


        Dictionary<string, List<(string fullTypeName, string fullName, TypeDeclarationSyntax typeDef)>> LocalCacheTypeSyntax = new(100);
        private void LoadTypeSyntax(IEnumerable<SyntaxTree> trees) {
            LocalCacheTypeSyntax.Clear();
            void ForEachChild(SyntaxNode node, string? parentName, string? parantTypeName) {
                if (node is CompilationUnitSyntax unit) {
                    foreach (var child in node.ChildNodes()) {
                        ForEachChild(child, null, null);
                    }
                }
                if (node is NamespaceDeclarationSyntax nd) {
                    if (parentName is null) {
                        parentName = nd.Name.ToString();
                    }
                    else {
                        parentName = $"{parentName}.{nd.Name}";
                    }
                    foreach (var child in node.ChildNodes()) {
                        ForEachChild(child, parentName, null);
                    }
                }
                else if (node is TypeDeclarationSyntax td) {
                    if (parentName is null) {
                        parentName = td.Identifier.Text;
                    }
                    else {
                        parentName = $"{parentName}.{td.Identifier.Text}";
                    }
                    if (parantTypeName is null) {
                        parantTypeName = td.Identifier.Text;
                    }
                    else {
                        parantTypeName = $"{parantTypeName}.{td.Identifier.Text}";
                    }
                    if (LocalCacheTypeSyntax.TryGetValue(td.Identifier.Text, out var list)) {
                        list.Add((parantTypeName, parentName, td));
                    }
                    else {
                        LocalCacheTypeSyntax.Add(td.Identifier.Text, new() { (parantTypeName, parentName, td) });
                    }
                    foreach (var child in node.ChildNodes()) {
                        ForEachChild(child, parentName, parantTypeName);
                    }
                }
            }
            foreach (var tree in trees) {
                if (tree.TryGetRoot(out var root)) {
                    ForEachChild(root, null, null);
                }
            }
        }
        public bool TryGetTypeDefSyntax(string name, [NotNullWhen(true)] out TypeDeclarationSyntax? type, string? currentNameSpace, string[] usings) {
            type = null;
            if (LocalCacheTypeSyntax.TryGetValue(name, out var types)) {
                if (types.Count == 0) return false;
                if (types.Count == 1) {
                    type = types[0].typeDef;
                    return true;
                }
                else {
                    foreach (var u in currentNameSpace is null ? usings : usings.Concat(new string[] { currentNameSpace })) {
                        foreach (var (fullTypeName, fullName, typeDef) in types) {
                            if (fullName == u + "." + name) {
                                type = typeDef;
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }
        public bool TryGetType(Predicate<(string fullTypeName, string fullName, TypeDeclarationSyntax typeDef)> predicate, [NotNullWhen(true)] out TypeDeclarationSyntax? firstMatch) {
            firstMatch = LocalCacheTypeSyntax.Values.SelectMany(list => list).FirstOrDefault(a => predicate(a)).typeDef;
            return firstMatch != null;
        }
    }
}
