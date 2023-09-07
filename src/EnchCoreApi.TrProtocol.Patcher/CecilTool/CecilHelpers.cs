using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;
using System.Reflection;

namespace EnchCoreApi.TrProtocol.Patcher.CecilTool {
    public static class CecilHelpers {
        public static TypeDefinition Type(this AssemblyDefinition assemblyDefinition, string name) {
            return assemblyDefinition.MainModule.Types.Single((TypeDefinition x) => x.FullName == name);
        }

        //
        // 摘要:
        //     Returns the TypeDefintion of the type specified
        public static TypeDefinition Type<T>(this AssemblyDefinition assemblyDefinition) {
            Type typeFromHandle = typeof(T);
            if (typeFromHandle.Assembly.FullName == assemblyDefinition.FullName) {
                return assemblyDefinition.Type(typeFromHandle.FullName);
            }

            throw new TypeAccessException(typeFromHandle.AssemblyQualifiedName + " is not a part of the current assembly definition");
        }

        public static Instruction Previous(this Instruction initial, Func<Instruction, bool> predicate) {
            while (initial.Previous != null) {
                if (predicate(initial)) {
                    return initial;
                }

                initial = initial.Previous;
            }

            return null;
        }

        public static Instruction Next(this Instruction initial, Func<Instruction, bool> predicate) {
            while (initial.Next != null) {
                if (predicate(initial.Next)) {
                    return initial.Next;
                }

                initial = initial.Next;
            }

            return null;
        }

        public static Instruction Previous(this Instruction initial, int count) {
            while (count > 0) {
                initial = initial.Previous;
                count--;
            }

            return initial;
        }

        public static List<Instruction> Next(this Instruction initial, int count = -1) {
            List<Instruction> list = new List<Instruction>();
            while (initial.Previous != null && (count == -1 || count > 0)) {
                initial = initial.Previous;
                count--;
                list.Add(initial);
            }

            return list;
        }

        //
        // 摘要:
        //     Returns a type from the current module by its fullName
        public static TypeDefinition Type(this ModuleDefinition moduleDefinition, string fullName) {
            return moduleDefinition.Types.Single((TypeDefinition x) => x.FullName == fullName);
        }

        //
        // 摘要:
        //     Enumerates all methods in the current module
        public static void ForEachMethod(this ModuleDefinition module, Action<MethodDefinition> callback) {
            module.ForEachType(delegate (TypeDefinition type) {
                foreach (MethodDefinition method in type.Methods) {
                    callback(method);
                }
            });
        }

        //
        // 摘要:
        //     Enumerates all instructions in all methods across each type of the assembly
        public static void ForEachInstruction(this ModuleDefinition module, Action<MethodDefinition, Instruction> callback) {
            module.ForEachMethod(delegate (MethodDefinition method) {
                if (method.HasBody) {
                    Instruction[] array = method.Body.Instructions.ToArray();
                    foreach (Instruction arg in array) {
                        callback(method, arg);
                    }
                }
            });
        }

        //
        // 摘要:
        //     Enumerates over each type in the assembly, including nested types
        public static void ForEachType(this ModuleDefinition module, Action<TypeDefinition> callback) {
            foreach (TypeDefinition type in module.Types) {
                callback(type);
                type.ForEachNestedType(callback);
            }
        }

        public static TypeDefinition TypeDefinition(this Type type, AssemblyDefinition definition) {
            return definition.Type(type.FullName);
        }

        public static MethodDefinition Method(this TypeDefinition typeDefinition, string name, Collection<ParameterDefinition> parameters, bool? isStatic = null, int skipMethodParameters = 0, int skipInputParameters = 0, bool acceptParamObjectTypes = false, bool substituteByRefs = false) {
            return typeDefinition.Method(name, isStatic, (ParameterTypeCollection)parameters, skipMethodParameters, skipInputParameters, acceptParamObjectTypes, substituteByRefs);
        }

        public static MethodDefinition Method(this TypeDefinition typeDefinition, string name, ParameterInfo[] parameters, bool? isStatic = null, int skipMethodParameters = 0, int skipInputParameters = 0, bool acceptParamObjectTypes = false, bool substituteByRefs = false) {
            return typeDefinition.Method(name, isStatic, (ParameterTypeCollection)parameters, skipMethodParameters, skipInputParameters, acceptParamObjectTypes, substituteByRefs);
        }

        public static MethodDefinition Method(this TypeDefinition typeDefinition, string name, bool? isStatic = null, ParameterTypeCollection parameters = null, int skipMethodParameters = 0, int skipInputParameters = 0, bool acceptParamObjectTypes = false, bool substituteByRefs = false) {
            IEnumerable<ParameterType> parametersClone = null;
            if (parameters != null) {
                if (skipInputParameters > 0) {
                    parametersClone = parameters.ToArray().Skip(skipInputParameters);
                }
                else {
                    parametersClone = parameters.ToArray();
                }
            }

            IEnumerable<MethodDefinition> source = typeDefinition.Methods.Where((MethodDefinition x) => x.Name == name && (!isStatic.HasValue || x.IsStatic == isStatic.Value));
            if (parameters != null) {
                source = source.Where((MethodDefinition x) => ((skipMethodParameters > 0) ? x.Parameters.Skip(skipMethodParameters).ToParameters() : x.Parameters.ToParameters()).CompareParameterTypes(parametersClone, acceptParamObjectTypes, substituteByRefs));
            }

            if (source.Count() == 0) {
                throw new Exception("Method `" + name + "` is not found in " + typeDefinition.FullName + ". Expected " + parametersClone.ToParamString() + ".");
            }

            if (source.Count() > 1) {
                throw new Exception("Too many methods named `" + name + "` found in " + typeDefinition.FullName);
            }

            return source.Single();
        }

        public static FieldDefinition Field(this TypeDefinition typeDefinition, string name) {
            return typeDefinition.Fields.Single((FieldDefinition x) => x.Name == name);
        }

        public static PropertyDefinition Property(this TypeDefinition typeDefinition, string name) {
            return typeDefinition.Properties.Single((PropertyDefinition x) => x.Name == name);
        }

        public static TypeDefinition NestedType(this TypeDefinition typeDefinition, string name) {
            return typeDefinition.NestedTypes.Single((TypeDefinition x) => x.Name == name);
        }

        public static bool CompareParameterTypes(this IEnumerable<ParameterType> source, IEnumerable<ParameterType> parameters, bool acceptParamObjectTypes = false, bool substituteByRefs = false) {
            if (source.Count() == parameters.Count()) {
                for (int i = 0; i < source.Count(); i++) {
                    ParameterType parameterType = source.ElementAt(i);
                    ParameterType parameterType2 = parameters.ElementAt(i);
                    if (substituteByRefs) {
                        ByReferenceType byReferenceType = parameterType.Type as ByReferenceType;
                        if (byReferenceType != null) {
                            parameterType = ParameterType.From(byReferenceType.ElementType);
                        }
                    }

                    if (parameterType.TypeName != parameterType2.TypeName && (!acceptParamObjectTypes || !(parameterType.Name == "Object"))) {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        public static void ForEachNestedType(this TypeDefinition parent, Action<TypeDefinition> callback) {
            foreach (TypeDefinition nestedType in parent.NestedTypes) {
                callback(nestedType);
                nestedType.ForEachNestedType(callback);
            }
        }

        //
        // 摘要:
        //     Returns the default constructor, expecting only one in the type.
        public static MethodDefinition Constructor(this TypeDefinition type) {
            return type.Method(".ctor");
        }

        //
        // 摘要:
        //     Returns the static constructor of the type, if any
        public static MethodDefinition StaticConstructor(this TypeDefinition type) {
            return type.Method(".cctor");
        }

        public static bool SignatureMatches(this TypeDefinition type, TypeDefinition compareTo) {
            IEnumerable<MethodDefinition> typeInstanceMethods = type.Methods.Where((MethodDefinition m) => !m.IsStatic && !m.IsGetter && !m.IsSetter);
            IEnumerable<MethodDefinition> source = compareTo.Methods.Where((MethodDefinition m) => !m.IsStatic && !m.IsGetter && !m.IsSetter && type.IsInterface && !m.IsConstructor);
            IEnumerable<MethodDefinition> enumerable = source.Where((MethodDefinition m) => !typeInstanceMethods.Any((MethodDefinition m2) => m2.Name == m.Name));
            if (typeInstanceMethods.Count() != source.Count()) {
                return false;
            }

            for (int i = 0; i < typeInstanceMethods.Count(); i++) {
                MethodDefinition method = typeInstanceMethods.ElementAt(i);
                MethodDefinition compareTo2 = source.ElementAt(i);
                if (!method.SignatureMatches(compareTo2)) {
                    return false;
                }
            }

            return true;
        }

        public static bool SignatureMatches(this MethodDefinition method, MethodDefinition compareTo, bool ignoreDeclaringType = true) {
            if (method.Name != compareTo.Name) {
                return false;
            }

            if (method.ReturnType.FullName != compareTo.ReturnType.FullName) {
                return false;
            }

            if (method.Parameters.Count != compareTo.Parameters.Count) {
                return false;
            }

            if (method.Overrides.Count != compareTo.Overrides.Count) {
                return false;
            }

            if (method.GenericParameters.Count != compareTo.GenericParameters.Count) {
                return false;
            }

            if (!method.DeclaringType.IsInterface && method.Attributes != compareTo.Attributes) {
                return false;
            }

            for (int i = 0; i < method.Parameters.Count; i++) {
                if (method.Parameters[i].ParameterType.FullName != compareTo.Parameters[i].ParameterType.FullName && ignoreDeclaringType && method.Parameters[i].ParameterType != method.DeclaringType) {
                    return false;
                }

                if (method.Parameters[i].Name != compareTo.Parameters[i].Name) {
                    return false;
                }
            }

            for (int j = 0; j < method.Overrides.Count; j++) {
                if (method.Overrides[j].Name != compareTo.Overrides[j].Name) {
                    return false;
                }
            }

            for (int k = 0; k < method.GenericParameters.Count; k++) {
                if (method.GenericParameters[k].Name != compareTo.GenericParameters[k].Name) {
                    return false;
                }
            }

            return true;
        }
    }
}
