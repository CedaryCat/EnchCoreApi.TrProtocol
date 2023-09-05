using Mono.Cecil;
using Mono.Cecil.Cil;

namespace EnchCoreApi.TrProtocol.Patcher.CecilTool {
    public static class Common {
        public static void MakeAllVirtual(this TypeDefinition type, params MethodDefinition[] ignores) {
            var methods = type.Methods.Where(m => !m.IsConstructor && !m.IsStatic).ToList();
            methods.AddRange(type.Properties.Select(p => p.SetMethod).Where(m => m != null && !m.IsStatic));
            methods.AddRange(type.Properties.Select(p => p.GetMethod).Where(m => m != null && !m.IsStatic));
            foreach (var method in methods) {
                if (ignores.Contains(method)) continue;

                if (method.Name != "cctor" && method.Name != "ctor" && !method.IsVirtual) {
                    method.IsVirtual = true;
                    method.IsNewSlot = true;
                }
            }

            type.Module.ForEachInstruction((method, instruction) => {
                if (methods.Any(x => x == instruction.Operand)) {
                    if (instruction.OpCode != OpCodes.Callvirt) {
                        instruction.OpCode = OpCodes.Callvirt;
                    }
                }
            });
        }
        public static void MakeAllDirect(this TypeDefinition type, params MethodDefinition[] ignores) {
            var methods = type.Methods.Where(m => !m.IsConstructor && !m.IsStatic).ToList();
            foreach (var method in methods) {
                if (ignores.Contains(method)) continue;

                if (method.Name != "cctor" && method.Name != "ctor" && !method.IsVirtual) {
                    //Create the new replacement method that will take place of the current method.
                    //So we must ensure we clone to meet the signatures.
                    var wrapped = new MethodDefinition(method.Name, method.Attributes, method.ReturnType);
                    var instanceMethod = (method.Attributes & Mono.Cecil.MethodAttributes.Static) == 0;


                    //Clone the parameters for the new method
                    if (method.HasParameters) {
                        foreach (var prm in method.Parameters) {
                            wrapped.Parameters.Add(prm);
                        }
                    }

                    //Rename the existing method, and replace all references to it so that the new 
                    //method receives the calls instead.
                    method.Name += "_Direct";
                    //Finally replace all instances of the current method with the wrapped method
                    //that is about to be generated
                    //Enumerates over each type in the assembly, including nested types
                    method.Module.ForEachInstruction((mth, ins) => {
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
                    if (wrapped.HasParameters) {
                        for (var i = 0; i < wrapped.Parameters.Count; i++) {
                            //Here we are looking at the callback to see if it wants a reference parameter.
                            //If it does, and it also expects an instance to be passed, we must move the offset
                            //by one to skip the previous ldarg_0 we added before.
                            //var offset = instanceMethod ? 1 : 0;
                            if (method.Parameters[i /*+ offset*/].ParameterType.IsByReference) {
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
        public static void MakeDirect(this TypeDefinition type, params MethodDefinition[] modifies) {
            foreach (var method in modifies.Where(m => !m.IsConstructor && !m.IsStatic && m.DeclaringType.FullName == type.FullName)) {
                if (method.Name != "cctor" && method.Name != "ctor" && !method.IsVirtual) {
                    //Create the new replacement method that will take place of the current method.
                    //So we must ensure we clone to meet the signatures.
                    var wrapped = new MethodDefinition(method.Name, method.Attributes, method.ReturnType);
                    var instanceMethod = (method.Attributes & Mono.Cecil.MethodAttributes.Static) == 0;


                    //Clone the parameters for the new method
                    if (method.HasParameters) {
                        foreach (var prm in method.Parameters) {
                            wrapped.Parameters.Add(prm);
                        }
                    }

                    //Rename the existing method, and replace all references to it so that the new 
                    //method receives the calls instead.
                    method.Name += "_Direct";
                    //Finally replace all instances of the current method with the wrapped method
                    //that is about to be generated
                    //Enumerates over each type in the assembly, including nested types
                    method.Module.ForEachInstruction((mth, ins) => {
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
                    if (wrapped.HasParameters) {
                        for (var i = 0; i < wrapped.Parameters.Count; i++) {
                            //Here we are looking at the callback to see if it wants a reference parameter.
                            //If it does, and it also expects an instance to be passed, we must move the offset
                            //by one to skip the previous ldarg_0 we added before.
                            //var offset = instanceMethod ? 1 : 0;
                            if (method.Parameters[i /*+ offset*/].ParameterType.IsByReference) {
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


        public static TypeReference? GetEnumUnderlyingType(this TypeDefinition type) {
            if (type.IsEnum) {
                return type.Fields.First(f => f.Name == "value__").FieldType;
            }
            return null;
        }
        public static IEnumerable<MethodDefinition> GetAllMethods(this TypeDefinition? type) {
            while (type != null) {
                foreach (var method in type.Methods) {
                    yield return method;
                }
                type = type.BaseType?.Resolve();
            }
        }
        public static unsafe OpCode? GetValueTypeConvertion(this TypeDefinition type, out bool isEnum, out int size) {
            var realType = type.GetEnumUnderlyingType();
            isEnum = true;
            if (realType is null) {
                isEnum = false;
                realType = type;
            }
            switch (realType.FullName) {
                case "System." + nameof(IntPtr): size = sizeof(IntPtr); return OpCodes.Conv_I;
                case "System." + nameof(SByte): size = sizeof(SByte); return OpCodes.Conv_I1;
                case "System." + nameof(Int16): size = sizeof(Int16); return OpCodes.Conv_I2;
                case "System." + nameof(Int32): size = sizeof(Int32); return OpCodes.Conv_I4;
                case "System." + nameof(Int64): size = sizeof(Int64); return OpCodes.Conv_I8;
                case "System." + nameof(UIntPtr): size = sizeof(UIntPtr); return OpCodes.Conv_U;
                case "System." + nameof(Byte): size = sizeof(Byte); return OpCodes.Conv_U1;
                case "System." + nameof(UInt16): size = sizeof(UInt16); return OpCodes.Conv_U2;
                case "System." + nameof(UInt32): size = sizeof(UInt32); return OpCodes.Conv_U4;
                case "System." + nameof(UInt64): size = sizeof(UInt64); return OpCodes.Conv_U8;
                case "System." + nameof(Single): size = sizeof(Single); return OpCodes.Conv_R4;
                case "System." + nameof(Double): size = sizeof(Double); return OpCodes.Conv_R8;
                case "System." + nameof(Decimal): size = sizeof(Decimal); return OpCodes.Conv_R_Un;
                default: size = 0; return null;
            }
        }
    }
}