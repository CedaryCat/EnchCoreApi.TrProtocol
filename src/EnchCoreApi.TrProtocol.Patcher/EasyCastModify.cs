using EnchCoreApi.TrProtocol.Attributes;
using EnchCoreApi.TrProtocol.Patcher.CecilTool;
using Fasterflect;
using ModFramework;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Utils;
using Assembly = System.Reflection.Assembly;

namespace EnchCoreApi.TrProtocol.Patcher {
    public class EasyCastModify : Modify {
        public override string Name => "Mod - EasyCast";

        public EasyCastModify(AssemblyDefinition destination, Assembly assembly) : base (destination, assembly) {
        }
        public override void Run(Logger logger) {

            foreach (var convertFrom_refleType in modelsAssembly.GetExportedTypes()) {
                var convert = System.Reflection.CustomAttributeExtensions.GetCustomAttribute<TypeConvertionAttribute>(convertFrom_refleType);
                if (convert is not null) {
                    logger.WriteLine($"正在搜索类型定义:'{convert.Type}'");
                    var convertTo_type = destination.MainModule.Types.FirstOrDefault(t => t.FullName == convert.Type);
                    if (convertTo_type is not null) {
                        var log = logger.WriteLine($"已检索-开始遍历成员生成浅表复制IL");

                        var convertFrom_typeRef = destination.MainModule.ImportReference(convertFrom_refleType);
                        var convertFrom_typeDef = convertFrom_typeRef.Resolve();


                        var opImplicit = convertTo_type.Methods.FirstOrDefault(m => m.Name == "op_Implicit" && m.CustomAttributes.Any(a => a.AttributeType.Name == nameof(CastOperatorPlaceHolderAttribute)));
                        var opExplicit = convertTo_type.Methods.FirstOrDefault(m => m.Name == "op_Explicit" && m.CustomAttributes.Any(a => a.AttributeType.Name == nameof(CastOperatorPlaceHolderAttribute)));


                        if (opImplicit is null) {
                            opImplicit = CreateCastOperator("op_Implicit", destination.MainModule, convertFrom_typeRef, convertTo_type);
                            if (opImplicit is null) {
                                continue;
                            }
                            convertTo_type.Methods.Add(opImplicit);
                        }
                        else {
                            opImplicit.Body = CreateCastOperatorBody(destination.MainModule, opImplicit);
                            opImplicit.CustomAttributes.Remove(opImplicit.CustomAttributes.First(a => a.AttributeType.Name == nameof(CastOperatorPlaceHolderAttribute)));
                        }

                        if (opExplicit is null) {
                            opExplicit = CreateCastOperator("op_Explicit", destination.MainModule, convertTo_type, convertFrom_typeDef);
                            if (opExplicit is null) {
                                continue;
                            }
                            convertTo_type.Methods.Add(opExplicit);
                        }
                        else {
                            opExplicit.Body = CreateCastOperatorBody(destination.MainModule, opExplicit);
                            opExplicit.CustomAttributes.Remove(opExplicit.CustomAttributes.First(a => a.AttributeType.Name == nameof(CastOperatorPlaceHolderAttribute)));
                        }

                        foreach (var convertFrom_member in convertFrom_refleType.GetMembers()) {
                            var att = System.Reflection.CustomAttributeExtensions.GetCustomAttribute<MemberConvertionAttribute>(convertFrom_member);
                            if (att is not null && att.Option != ConvertionOption.Ignore) {

                                if (convertFrom_member is System.Reflection.PropertyInfo convertFrom_refleProp) {

                                    logger.WriteLine($"|映射源:{convertFrom_refleType.FullName}|Property:{convertFrom_member.Name}[类型:{convertFrom_refleProp.PropertyType.FullName}]", log);
                                    logger.WriteLine($"查找对应映射ing", log);

                                    if (convertFrom_refleProp.GetMethod is null || convertFrom_refleProp.SetMethod is null) {
                                        throw new Exception($"property '{convertFrom_refleProp.Name}' accesstors not defined");
                                    }
                                    var convertTo_prop = convertTo_type.Properties.FirstOrDefault(p => p.Name == convertFrom_member.Name && p.GetMethod?.Body is not null && p.SetMethod?.Body is not null);

                                    if (convertTo_prop is not null) {

                                        if (att.Option == ConvertionOption.Custom) {
                                            if (att.CustomConvertionFromMethod is null) {
                                                logger.WriteLineError($"自定义成员转换的{nameof(att.CustomConvertionFromMethod)}为null --press key 以继续", log);
                                                Console.ReadKey();
                                                continue;
                                            }
                                            else {
                                                var convMethod = convertTo_type.Methods.FirstOrDefault(m => m.Name == att.CustomConvertionFromMethod);
                                                if (convMethod is null) {
                                                    logger.WriteLineError($"自定义成员转换的{nameof(att.CustomConvertionFromMethod)}'{att.CustomConvertionFromMethod}'不存在 --press key 以继续", log);
                                                    Console.ReadKey();
                                                    continue;
                                                }
                                                else {
                                                    opImplicit.Body.Instructions.Add(Instruction.Create(OpCodes.Dup));
                                                    opImplicit.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg, opImplicit.Parameters[0]));
                                                    opImplicit.Body.Instructions.Add(Instruction.Create(OpCodes.Callvirt, destination.MainModule.ImportReference(convertFrom_refleProp.GetMethod)));
                                                    opImplicit.Body.Instructions.Add(Instruction.Create(OpCodes.Call, convMethod));
                                                    opImplicit.Body.Instructions.Add(Instruction.Create(OpCodes.Callvirt, convertTo_prop.SetMethod));
                                                }
                                            }
                                            if (att.CustomConvertionToMethod is null) {
                                                logger.WriteLineError($"自定义成员转换的{nameof(att.CustomConvertionToMethod)}为null --press key 以继续", log);
                                                Console.ReadKey();
                                                continue;
                                            }
                                            else {
                                                var convMethod = convertTo_type.Methods.FirstOrDefault(m => m.Name == att.CustomConvertionToMethod);
                                                if (convMethod is null) {
                                                    logger.WriteLineError($"自定义成员转换的{nameof(att.CustomConvertionToMethod)}'{att.CustomConvertionToMethod}'不存在 --press key 以继续", log);
                                                    Console.ReadKey();
                                                    continue;
                                                }
                                                else {
                                                    opExplicit.Body.Instructions.Add(Instruction.Create(OpCodes.Dup));
                                                    opExplicit.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg, opImplicit.Parameters[0]));
                                                    opExplicit.Body.Instructions.Add(Instruction.Create(OpCodes.Callvirt, convertTo_prop.GetMethod));
                                                    opExplicit.Body.Instructions.Add(Instruction.Create(OpCodes.Call, convMethod));
                                                    opExplicit.Body.Instructions.Add(Instruction.Create(OpCodes.Callvirt, destination.MainModule.ImportReference(convertFrom_refleProp.SetMethod)));
                                                }
                                            }
                                        }
                                        else {

                                            var convertFrom_prop_typeRef = destination.MainModule.ImportReference(convertFrom_refleProp.PropertyType);
                                            var convertFrom_prop_typeDef = convertFrom_prop_typeRef.Resolve();

                                            WriteMemberCopyIL(
                                                opImplicit,
                                                destination.MainModule,
                                                convertFrom_prop_typeDef,
                                                convertTo_prop.PropertyType.Resolve(),
                                                () => Instruction.Create(OpCodes.Callvirt, destination.MainModule.ImportReference(convertFrom_refleProp.GetMethod)),
                                                () => Instruction.Create(OpCodes.Callvirt, convertTo_prop.SetMethod),
                                                logger,
                                                log);
                                            WriteMemberCopyIL(
                                                opExplicit,
                                                destination.MainModule,
                                                convertTo_prop.PropertyType.Resolve(),
                                                convertFrom_prop_typeDef,
                                                () => Instruction.Create(OpCodes.Callvirt, convertTo_prop.GetMethod),
                                                () => Instruction.Create(OpCodes.Callvirt, destination.MainModule.ImportReference(convertFrom_refleProp.SetMethod)),
                                                logger,
                                                log);
                                        }
                                    }
                                    else {
                                        logger.WriteLine($"无法在'{convertFrom_refleType.Name}'中查找得对应映射属性'{convertFrom_member.Name}' --press key 以继续", log);
                                        Console.ReadKey();
                                    }
                                }
                                else {
                                    var convertFrom_refleField = (System.Reflection.FieldInfo)convertFrom_member;

                                    logger.WriteLine($"|映射源:{convertFrom_refleType.FullName}|Field:{convertFrom_member.Name}[类型:{convertFrom_refleField.FieldType.FullName}]", log);

                                    var convertTo_Field = convertTo_type.Fields.FirstOrDefault(f => f.Name == convertFrom_member.Name);

                                    if (convertTo_Field is not null) {

                                        var convertFrom_field = destination.MainModule.ImportReference(convertFrom_refleField);

                                        if (att.Option == ConvertionOption.Custom) {
                                            if (att.CustomConvertionFromMethod is null) {
                                                logger.WriteLineError($"自定义成员转换的{nameof(att.CustomConvertionFromMethod)}为null --press key 以继续", log);
                                                Console.ReadKey();
                                                continue;
                                            }
                                            else {
                                                var convMethod = convertTo_type.Methods.FirstOrDefault(m => m.Name == att.CustomConvertionFromMethod);
                                                if (convMethod is null) {
                                                    logger.WriteLineError($"自定义成员转换的{nameof(att.CustomConvertionFromMethod)}'{att.CustomConvertionFromMethod}'不存在 --press key 以继续", log);
                                                    Console.ReadKey();
                                                    continue;
                                                }
                                                else {
                                                    opImplicit.Body.Instructions.Add(Instruction.Create(OpCodes.Dup));
                                                    opImplicit.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg, opImplicit.Parameters[0]));
                                                    opImplicit.Body.Instructions.Add(Instruction.Create(OpCodes.Ldfld, convertFrom_field));
                                                    opImplicit.Body.Instructions.Add(Instruction.Create(OpCodes.Call, convMethod));
                                                    opImplicit.Body.Instructions.Add(Instruction.Create(OpCodes.Stfld, convertTo_Field));
                                                }
                                            }
                                            if (att.CustomConvertionToMethod is null) {
                                                logger.WriteLineError($"自定义成员转换的{nameof(att.CustomConvertionToMethod)}为null --press key 以继续", log);
                                                Console.ReadKey();
                                                continue;
                                            }
                                            else {
                                                var convMethod = convertTo_type.Methods.FirstOrDefault(m => m.Name == att.CustomConvertionToMethod);
                                                if (convMethod is null) {
                                                    logger.WriteLineError($"自定义成员转换的{nameof(att.CustomConvertionToMethod)}'{att.CustomConvertionToMethod}'不存在 --press key 以继续", log);
                                                    Console.ReadKey();
                                                    continue;
                                                }
                                                else {
                                                    opExplicit.Body.Instructions.Add(Instruction.Create(OpCodes.Dup));
                                                    opExplicit.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg, opImplicit.Parameters[0]));
                                                    opExplicit.Body.Instructions.Add(Instruction.Create(OpCodes.Ldfld, convertTo_Field));
                                                    opExplicit.Body.Instructions.Add(Instruction.Create(OpCodes.Call, convMethod));
                                                    opExplicit.Body.Instructions.Add(Instruction.Create(OpCodes.Stfld, convertFrom_field));
                                                }
                                            }
                                        }
                                        else {

                                            var convertFrom_field_typeRef = convertFrom_field.FieldType;
                                            var convertFrom_field_typeDef = convertFrom_field_typeRef.Resolve();

                                            WriteMemberCopyIL(
                                                opImplicit,
                                                destination.MainModule,
                                                convertFrom_field_typeDef,
                                                convertTo_Field.FieldType.Resolve(),
                                                () => Instruction.Create(OpCodes.Ldfld, convertFrom_field),
                                                () => Instruction.Create(OpCodes.Stfld, convertTo_Field),
                                                logger,
                                                log);
                                            WriteMemberCopyIL(
                                                opExplicit,
                                                destination.MainModule,
                                                convertTo_Field.FieldType.Resolve(),
                                                convertFrom_field_typeDef,
                                                () => Instruction.Create(OpCodes.Ldfld, convertTo_Field),
                                                () => Instruction.Create(OpCodes.Stfld, convertFrom_field),
                                                logger,
                                                log);
                                        }
                                    }
                                    else {
                                        logger.WriteLine($"无法在'{convertFrom_refleType.Name}'中查找得对应映射属性'{convertFrom_member.Name}' --press key 以继续", log);
                                        Console.ReadKey();
                                    }
                                }
                            }
                        }

                        opImplicit.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
                        opExplicit.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
                    }
                    else {
                        logger.WriteLine($"未查找到类型定义:'{convert.Type}' --press key 以继续");
                        Console.ReadKey();
                    }
                }
            }
        }

        static void WriteMemberCopyIL(MethodDefinition opMethod, ModuleDefinition main, TypeDefinition memberTypeFrom, TypeDefinition memberTypeTo, Func<Instruction> getValue, Func<Instruction> setValue, Logger logger, LogData parentLog) {
            if (memberTypeTo.FullName == memberTypeFrom.FullName) {
                logger.WriteLine($"已确定'{opMethod.Name}'映射关系<简单复制>: {memberTypeFrom.FullName} => {memberTypeTo.FullName}", parentLog);
                opMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Dup));
                opMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg, opMethod.Parameters[0]));
                opMethod.Body.Instructions.Add(getValue());
                opMethod.Body.Instructions.Add(setValue());
                return;
            }
            else {

                var convertTo_member_convOp = memberTypeTo.GetValueTypeConvertion(out var convertTo_member_isEnum, out _);
                var convertFrom_member_convOp = memberTypeFrom.GetValueTypeConvertion(out var convertFrom_member_isEnum, out _);

                

                foreach (var convertMethod in memberTypeFrom.GetAllMethods().Where(m => m.IsStatic && m.IsHideBySig && m.IsSpecialName && m.Name is "op_Explicit" || m.Name == "op_Implicit")) {
                    if (convertMethod.Parameters[0].ParameterType.FullName == memberTypeFrom.FullName && convertMethod.ReturnType.FullName == memberTypeTo.FullName) {

                        logger.WriteLine($"已确定'{opMethod.Name}'映射关系<CastOp重载>: ({convertMethod.Name})({memberTypeFrom.FullName}) => {memberTypeTo.FullName}", parentLog);

                        opMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Dup));
                        opMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg, opMethod.Parameters[0]));
                        opMethod.Body.Instructions.Add(getValue());
                        opMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Call, convertMethod));
                        opMethod.Body.Instructions.Add(setValue());
                        return;
                    }
                }

                if (convertTo_member_convOp is not null) {
                    //use conv.xx opcode
                    if (convertFrom_member_convOp is not null) {
                        logger.WriteLine($"已确定'{opMethod.Name}'映射关系<conv>: ({convertFrom_member_convOp})({memberTypeFrom.FullName}) => ({convertTo_member_convOp})({memberTypeTo.FullName})", parentLog);
                        opMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Dup));
                        opMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg, opMethod.Parameters[0]));
                        opMethod.Body.Instructions.Add(getValue());
                        opMethod.Body.Instructions.Add(Instruction.Create(convertTo_member_convOp.Value));
                        opMethod.Body.Instructions.Add(setValue());
                        return;
                    }
                    //use explict or implict
                    else {
                        foreach (var convertMethod in memberTypeFrom.GetAllMethods().Where(m => m.IsStatic && m.IsHideBySig && m.IsSpecialName && m.Name is "op_Explicit" || m.Name == "op_Implicit")) {
                            if (convertMethod.Parameters[0].ParameterType.FullName == memberTypeFrom.FullName && convertMethod.ReturnType.Resolve().GetValueTypeConvertion(out _, out _) == convertTo_member_convOp) {
                                Console.WriteLine($"已确定'{opMethod.Name}'映射关系<CastOp-conv>: ({convertMethod.Name}:{convertTo_member_convOp})({memberTypeFrom.FullName}) => ({convertTo_member_convOp})({memberTypeTo.FullName})", parentLog);
                                opMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Dup));
                                opMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg, opMethod.Parameters[0]));
                                opMethod.Body.Instructions.Add(getValue());
                                opMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Call, convertMethod));
                                opMethod.Body.Instructions.Add(setValue());
                                return;
                            }
                        }
                        var log = logger.WriteLine($"未查找到明确指定'{opMethod.Name}'映射路径({memberTypeFrom.FullName}) - ({memberTypeTo.FullName})，尝试模糊查找", parentLog);
                        MethodDefinition? convertMethod_smallest = null;
                        int currentSize = sizeof(Decimal) + 1;
                        foreach (var convertMethod in memberTypeFrom.GetAllMethods().Where(m => m.IsStatic && m.IsHideBySig && m.IsSpecialName && m.Name is "op_Explicit" || m.Name == "op_Implicit")) {
                            if (convertMethod.Parameters[0].ParameterType.FullName == memberTypeFrom.FullName && (convertFrom_member_convOp = convertMethod.ReturnType.Resolve().GetValueTypeConvertion(out _, out var size)) != null) {
                                Console.WriteLine($"模糊查找得路径(size = {size} | current = {currentSize}) ({convertMethod.Name}:{convertFrom_member_convOp})({memberTypeFrom.FullName}) => ({convertTo_member_convOp})({memberTypeTo.FullName})", log);
                                if (size < currentSize) {
                                    convertMethod_smallest = convertMethod;
                                    currentSize = size;
                                    Console.WriteLine($"已更新为当前优选路径(size = {size})", log);
                                }
                            }
                        }
                        if (convertMethod_smallest is not null) {
                            opMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Dup));
                            opMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg, opMethod.Parameters[0]));
                            opMethod.Body.Instructions.Add(getValue());
                            opMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Call, convertMethod_smallest));
                            opMethod.Body.Instructions.Add(Instruction.Create(convertTo_member_convOp.Value));
                            opMethod.Body.Instructions.Add(setValue());
                            return;
                        }
                        else {
                            logger.WriteLine($"未查找到'{opMethod.Name}'任何匹配路径 --press key 以继续", parentLog);
                            Console.ReadKey();
                        }
                    }
                }
                else {
                    logger.WriteLine($"未查找到'{opMethod.Name}'任何匹配路径 --press key 以继续", parentLog);
                    Console.ReadKey();
                }
            }
        }

        static MethodDefinition? CreateCastOperator(string operatorName, ModuleDefinition main, TypeReference typeFrom, TypeDefinition typeTo) {

            var op = new MethodDefinition(operatorName, MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.Static, main.ImportReference(typeTo));
            var opImplict_param = new ParameterDefinition("a", ParameterAttributes.None, typeFrom);
            op.Parameters.Add(opImplict_param);

            op.Body = CreateCastOperatorBody(main, op);

            return op;
        }
        static MethodBody CreateCastOperatorBody(ModuleDefinition main, MethodDefinition method) {
            var body = new MethodBody(method);
            var typeTo = method.ReturnType.Resolve();

            bool typeToIsInLocalAssembly = typeTo.Module.FileName == main.FileName;
            if (method.ReturnType.IsValueType) {
                body.Instructions.Add(Instruction.Create(OpCodes.Initobj, typeTo));
            }
            else {
                var convertTo_ctor = typeTo.Methods.FirstOrDefault(m => m.IsConstructor && m.Parameters.Count == 0);
                if (convertTo_ctor is null) {
                    if (typeToIsInLocalAssembly) {
                        var methods = typeTo.BaseType.Resolve().Methods;
                        var convertTo_ctor_base = methods.FirstOrDefault(m => !m.IsPrivate && m.IsConstructor && m.Parameters.Count == 0);

                        if (convertTo_ctor_base is null) {
                            Console.WriteLine($"could not find any non-params ctor in base type '{typeTo.BaseType}' -- press key to continue");
                            Console.ReadKey();
                            return null;
                        }

                        convertTo_ctor = new MethodDefinition(".ctor", MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName, main.TypeSystem.Void);
                        convertTo_ctor.Body = new MethodBody(convertTo_ctor);
                        convertTo_ctor.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                        convertTo_ctor.Body.Instructions.Add(Instruction.Create(OpCodes.Newobj, main.ImportReference(convertTo_ctor_base)));
                        convertTo_ctor.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
                    }
                    else {
                        Console.WriteLine($"could not find any non-params ctor in type '{typeTo}' -- press key to continue");
                        Console.ReadKey();
                    }
                }
                body.Instructions.Add(Instruction.Create(OpCodes.Newobj, main.ImportReference(convertTo_ctor)));
            }

            return body;
        }
    }
}
