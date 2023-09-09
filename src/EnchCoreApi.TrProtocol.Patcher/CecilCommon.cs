using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;
using System.Reflection;

namespace EnchCoreApi.TrProtocol.Patcher
{
    [MonoMod.MonoModIgnore]
    public static class CecilCommon
    {
        public static TypeDefinition Type(this AssemblyDefinition assemblyDefinition, string name)
        {
            return assemblyDefinition.MainModule.Types.Single((x) => x.FullName == name);
        }

        //
        // 摘要:
        //     Returns the TypeDefintion of the type specified
        public static TypeDefinition Type<T>(this AssemblyDefinition assemblyDefinition)
        {
            Type typeFromHandle = typeof(T);
            if (typeFromHandle.Assembly.FullName == assemblyDefinition.FullName)
            {
                return assemblyDefinition.Type(typeFromHandle.FullName);
            }

            throw new TypeAccessException(typeFromHandle.AssemblyQualifiedName + " is not a part of the current assembly definition");
        }

        public static Instruction Previous(this Instruction initial, Func<Instruction, bool> predicate)
        {
            while (initial.Previous != null)
            {
                if (predicate(initial))
                {
                    return initial;
                }

                initial = initial.Previous;
            }

            return null;
        }

        public static Instruction Next(this Instruction initial, Func<Instruction, bool> predicate)
        {
            while (initial.Next != null)
            {
                if (predicate(initial.Next))
                {
                    return initial.Next;
                }

                initial = initial.Next;
            }

            return null;
        }

        public static Instruction Previous(this Instruction initial, int count)
        {
            while (count > 0)
            {
                initial = initial.Previous;
                count--;
            }

            return initial;
        }

        public static List<Instruction> Next(this Instruction initial, int count = -1)
        {
            List<Instruction> list = new List<Instruction>();
            while (initial.Previous != null && (count == -1 || count > 0))
            {
                initial = initial.Previous;
                count--;
                list.Add(initial);
            }

            return list;
        }

        //
        // 摘要:
        //     Returns a type from the current module by its fullName
        public static TypeDefinition Type(this ModuleDefinition moduleDefinition, string fullName)
        {
            return moduleDefinition.Types.Single((x) => x.FullName == fullName);
        }

        //
        // 摘要:
        //     Enumerates all methods in the current module
        public static void ForEachMethod(this ModuleDefinition module, Action<MethodDefinition> callback)
        {
            module.ForEachType(delegate (TypeDefinition type)
            {
                foreach (MethodDefinition method in type.Methods)
                {
                    callback(method);
                }
            });
        }

        //
        // 摘要:
        //     Enumerates all instructions in all methods across each type of the assembly
        public static void ForEachInstruction(this ModuleDefinition module, Action<MethodDefinition, Instruction> callback)
        {
            module.ForEachMethod(delegate (MethodDefinition method)
            {
                if (method.HasBody)
                {
                    Instruction[] array = method.Body.Instructions.ToArray();
                    foreach (Instruction arg in array)
                    {
                        callback(method, arg);
                    }
                }
            });
        }

        //
        // 摘要:
        //     Enumerates over each type in the assembly, including nested types
        public static void ForEachType(this ModuleDefinition module, Action<TypeDefinition> callback)
        {
            foreach (TypeDefinition type in module.Types)
            {
                callback(type);
                type.ForEachNestedType(callback);
            }
        }

        public static TypeDefinition TypeDefinition(this Type type, AssemblyDefinition definition)
        {
            return definition.Type(type.FullName);
        }
        public static FieldDefinition Field(this TypeDefinition typeDefinition, string name)
        {
            return typeDefinition.Fields.Single((x) => x.Name == name);
        }

        public static PropertyDefinition Property(this TypeDefinition typeDefinition, string name)
        {
            return typeDefinition.Properties.Single((x) => x.Name == name);
        }

        public static TypeDefinition NestedType(this TypeDefinition typeDefinition, string name)
        {
            return typeDefinition.NestedTypes.Single((x) => x.Name == name);
        }
        public static void ForEachNestedType(this TypeDefinition parent, Action<TypeDefinition> callback)
        {
            foreach (TypeDefinition nestedType in parent.NestedTypes)
            {
                callback(nestedType);
                nestedType.ForEachNestedType(callback);
            }
        }

        public static bool SignatureMatches(this TypeDefinition type, TypeDefinition compareTo)
        {
            IEnumerable<MethodDefinition> typeInstanceMethods = type.Methods.Where((m) => !m.IsStatic && !m.IsGetter && !m.IsSetter);
            IEnumerable<MethodDefinition> source = compareTo.Methods.Where((m) => !m.IsStatic && !m.IsGetter && !m.IsSetter && type.IsInterface && !m.IsConstructor);
            IEnumerable<MethodDefinition> enumerable = source.Where((m) => !typeInstanceMethods.Any((m2) => m2.Name == m.Name));
            if (typeInstanceMethods.Count() != source.Count())
            {
                return false;
            }

            for (int i = 0; i < typeInstanceMethods.Count(); i++)
            {
                MethodDefinition method = typeInstanceMethods.ElementAt(i);
                MethodDefinition compareTo2 = source.ElementAt(i);
                if (!method.SignatureMatches(compareTo2))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool SignatureMatches(this MethodDefinition method, MethodDefinition compareTo, bool ignoreDeclaringType = true)
        {
            if (method.Name != compareTo.Name)
            {
                return false;
            }

            if (method.ReturnType.FullName != compareTo.ReturnType.FullName)
            {
                return false;
            }

            if (method.Parameters.Count != compareTo.Parameters.Count)
            {
                return false;
            }

            if (method.Overrides.Count != compareTo.Overrides.Count)
            {
                return false;
            }

            if (method.GenericParameters.Count != compareTo.GenericParameters.Count)
            {
                return false;
            }

            if (!method.DeclaringType.IsInterface && method.Attributes != compareTo.Attributes)
            {
                return false;
            }

            for (int i = 0; i < method.Parameters.Count; i++)
            {
                if (method.Parameters[i].ParameterType.FullName != compareTo.Parameters[i].ParameterType.FullName && ignoreDeclaringType && method.Parameters[i].ParameterType != method.DeclaringType)
                {
                    return false;
                }

                if (method.Parameters[i].Name != compareTo.Parameters[i].Name)
                {
                    return false;
                }
            }

            for (int j = 0; j < method.Overrides.Count; j++)
            {
                if (method.Overrides[j].Name != compareTo.Overrides[j].Name)
                {
                    return false;
                }
            }

            for (int k = 0; k < method.GenericParameters.Count; k++)
            {
                if (method.GenericParameters[k].Name != compareTo.GenericParameters[k].Name)
                {
                    return false;
                }
            }

            return true;
        }


        public static void MakeAllVirtual(this TypeDefinition type, params MethodDefinition[] ignores)
        {
            var methods = type.Methods.Where(m => !m.IsConstructor && !m.IsStatic).ToList();
            methods.AddRange(type.Properties.Select(p => p.SetMethod).Where(m => m != null && !m.IsStatic));
            methods.AddRange(type.Properties.Select(p => p.GetMethod).Where(m => m != null && !m.IsStatic));
            foreach (var method in methods)
            {
                if (ignores.Contains(method)) continue;

                if (method.Name != "cctor" && method.Name != "ctor" && !method.IsVirtual)
                {
                    method.IsVirtual = true;
                    method.IsNewSlot = true;
                }
            }

            type.Module.ForEachInstruction((method, instruction) =>
            {
                if (methods.Any(x => x == instruction.Operand))
                {
                    if (instruction.OpCode != OpCodes.Callvirt)
                    {
                        instruction.OpCode = OpCodes.Callvirt;
                    }
                }
            });
        }
        public static void MakeAllDirect(this TypeDefinition type, params MethodDefinition[] ignores)
        {
            var methods = type.Methods.Where(m => !m.IsConstructor && !m.IsStatic).ToList();
            foreach (var method in methods)
            {
                if (ignores.Contains(method)) continue;

                if (method.Name != "cctor" && method.Name != "ctor" && !method.IsVirtual)
                {
                    //Create the new replacement method that will take place of the current method.
                    //So we must ensure we clone to meet the signatures.
                    var wrapped = new MethodDefinition(method.Name, method.Attributes, method.ReturnType);
                    var instanceMethod = (method.Attributes & Mono.Cecil.MethodAttributes.Static) == 0;


                    //Clone the parameters for the new method
                    if (method.HasParameters)
                    {
                        foreach (var prm in method.Parameters)
                        {
                            wrapped.Parameters.Add(prm);
                        }
                    }

                    //Rename the existing method, and replace all references to it so that the new 
                    //method receives the calls instead.
                    method.Name += "_Direct";
                    //Finally replace all instances of the current method with the wrapped method
                    //that is about to be generated
                    //Enumerates over each type in the assembly, including nested types
                    method.Module.ForEachInstruction((mth, ins) =>
                    {
                        //Compare each instruction operand value as if it were a method reference. Check to 
                        //see if they match the current method definition. If it matches, it can be swapped.
                        if (ins.Operand == method)
                            ins.Operand = wrapped;
                    });

                    //Get the il processor instance so we can modify IL
                    var il = wrapped.Body.GetILProcessor();

                    //If the callback expects the instance, emit 'this'
                    if (instanceMethod)
                        il.Emit(OpCodes.Ldarg_0);

                    //If there are parameters, add each of them to the stack for the callback
                    if (wrapped.HasParameters)
                    {
                        for (var i = 0; i < wrapped.Parameters.Count; i++)
                        {
                            //Here we are looking at the callback to see if it wants a reference parameter.
                            //If it does, and it also expects an instance to be passed, we must move the offset
                            //by one to skip the previous ldarg_0 we added before.
                            //var offset = instanceMethod ? 1 : 0;
                            if (method.Parameters[i /*+ offset*/].ParameterType.IsByReference)
                            {
                                il.Emit(OpCodes.Ldarga, wrapped.Parameters[i]);
                            }
                            else il.Emit(OpCodes.Ldarg, wrapped.Parameters[i]);
                        }
                    }

                    //Execute the callback
                    il.Emit(OpCodes.Call, method);

                    //If the end call has a value, pop it for the time being.
                    //In the case of begin callbacks, we use this value to determine
                    //a cancel.
                    //if (method.ReturnType.Name != method.Module.TypeSystem.Void.Name)
                    //    il.Emit(OpCodes.Pop);

                    il.Emit(OpCodes.Ret);

                    //Place the new method in the declaring type of the method we are cloning
                    method.DeclaringType.Methods.Add(wrapped);
                }
            }
        }
        public static void MakeDirect(this TypeDefinition type, params MethodDefinition[] modifies)
        {
            foreach (var method in modifies.Where(m => !m.IsConstructor && !m.IsStatic && m.DeclaringType.FullName == type.FullName))
            {
                if (method.Name != "cctor" && method.Name != "ctor" && !method.IsVirtual)
                {
                    //Create the new replacement method that will take place of the current method.
                    //So we must ensure we clone to meet the signatures.
                    var wrapped = new MethodDefinition(method.Name, method.Attributes, method.ReturnType);
                    var instanceMethod = (method.Attributes & Mono.Cecil.MethodAttributes.Static) == 0;


                    //Clone the parameters for the new method
                    if (method.HasParameters)
                    {
                        foreach (var prm in method.Parameters)
                        {
                            wrapped.Parameters.Add(prm);
                        }
                    }

                    //Rename the existing method, and replace all references to it so that the new 
                    //method receives the calls instead.
                    method.Name += "_Direct";
                    //Finally replace all instances of the current method with the wrapped method
                    //that is about to be generated
                    //Enumerates over each type in the assembly, including nested types
                    method.Module.ForEachInstruction((mth, ins) =>
                    {
                        //Compare each instruction operand value as if it were a method reference. Check to 
                        //see if they match the current method definition. If it matches, it can be swapped.
                        if (ins.Operand == method)
                            ins.Operand = wrapped;
                    });

                    //Get the il processor instance so we can modify IL
                    var il = wrapped.Body.GetILProcessor();

                    //If the callback expects the instance, emit 'this'
                    if (instanceMethod)
                        il.Emit(OpCodes.Ldarg_0);

                    //If there are parameters, add each of them to the stack for the callback
                    if (wrapped.HasParameters)
                    {
                        for (var i = 0; i < wrapped.Parameters.Count; i++)
                        {
                            //Here we are looking at the callback to see if it wants a reference parameter.
                            //If it does, and it also expects an instance to be passed, we must move the offset
                            //by one to skip the previous ldarg_0 we added before.
                            //var offset = instanceMethod ? 1 : 0;
                            if (method.Parameters[i /*+ offset*/].ParameterType.IsByReference)
                            {
                                il.Emit(OpCodes.Ldarga, wrapped.Parameters[i]);
                            }
                            else il.Emit(OpCodes.Ldarg, wrapped.Parameters[i]);
                        }
                    }

                    //Execute the callback
                    il.Emit(OpCodes.Call, method);

                    //If the end call has a value, pop it for the time being.
                    //In the case of begin callbacks, we use this value to determine
                    //a cancel.
                    //if (method.ReturnType.Name != method.Module.TypeSystem.Void.Name)
                    //    il.Emit(OpCodes.Pop);

                    il.Emit(OpCodes.Ret);

                    //Place the new method in the declaring type of the method we are cloning
                    method.DeclaringType.Methods.Add(wrapped);
                }
            }
        }


        public static TypeReference? GetEnumUnderlyingType(this TypeDefinition type)
        {
            if (type.IsEnum)
            {
                return type.Fields.First(f => f.Name == "value__").FieldType;
            }
            return null;
        }
        public static IEnumerable<MethodDefinition> GetAllMethods(this TypeDefinition? type)
        {
            while (type != null)
            {
                foreach (var method in type.Methods)
                {
                    yield return method;
                }
                type = type.BaseType?.Resolve();
            }
        }
        public static unsafe OpCode? GetValueTypeConvertion(this TypeDefinition type, out bool isEnum, out int size)
        {
            var realType = type.GetEnumUnderlyingType();
            isEnum = true;
            if (realType is null)
            {
                isEnum = false;
                realType = type;
            }
            switch (realType.FullName)
            {
                case "System." + nameof(IntPtr): size = sizeof(IntPtr); return OpCodes.Conv_I;
                case "System." + nameof(SByte): size = sizeof(sbyte); return OpCodes.Conv_I1;
                case "System." + nameof(Int16): size = sizeof(short); return OpCodes.Conv_I2;
                case "System." + nameof(Int32): size = sizeof(int); return OpCodes.Conv_I4;
                case "System." + nameof(Int64): size = sizeof(long); return OpCodes.Conv_I8;
                case "System." + nameof(UIntPtr): size = sizeof(UIntPtr); return OpCodes.Conv_U;
                case "System." + nameof(Byte): size = sizeof(byte); return OpCodes.Conv_U1;
                case "System." + nameof(UInt16): size = sizeof(ushort); return OpCodes.Conv_U2;
                case "System." + nameof(UInt32): size = sizeof(uint); return OpCodes.Conv_U4;
                case "System." + nameof(UInt64): size = sizeof(ulong); return OpCodes.Conv_U8;
                case "System." + nameof(Single): size = sizeof(float); return OpCodes.Conv_R4;
                case "System." + nameof(Double): size = sizeof(double); return OpCodes.Conv_R8;
                case "System." + nameof(Decimal): size = sizeof(decimal); return OpCodes.Conv_R_Un;
                default: size = 0; return null;
            }
        }
    }
}
