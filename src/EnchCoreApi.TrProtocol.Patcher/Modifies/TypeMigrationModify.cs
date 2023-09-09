using EnchCoreApi.TrProtocol.Attributes;
using ModFramework;
using Mono.Cecil;
using MonoMod.Utils;
using Assembly = System.Reflection.Assembly;

namespace EnchCoreApi.TrProtocol.Patcher.Modifies
{
    [MonoMod.MonoModIgnore]
    public class TypeMigrationModify : Modify
    {
        public TypeMigrationModify(ModFwModder modder, Assembly modify) : base(modder.Module.Assembly, modify)
        {
            forwardeds = new List<Type>();
        }
        public IReadOnlyList<Type> Forwardeds => forwardeds;
        private readonly List<Type> forwardeds;
        public override string Name => "Mod - Type Migration";

        public override void Run(Logger logger)
        {

            foreach (var type in modelsAssembly.GetExportedTypes().Where(t => System.Reflection.CustomAttributeExtensions.GetCustomAttribute<TypeMigrationTargetAttribute>(t) is not null))
            {
                var origiType = destination.MainModule.Types.FirstOrDefault(t => t.FullName == type.FullName);
                if (origiType != null)
                {

                    destination.MainModule.Types.Remove(origiType);
                    var tRef = destination.MainModule.ImportReference(type);
                    var tDef = tRef.Resolve();

                    var forwardType = new ExportedType(tRef.Namespace, tRef.Name, tRef.Module, tRef.Scope);
                    forwardType.Attributes = TypeAttributes.Forwarder;
                    destination.MainModule.ExportedTypes.Add(forwardType);
                    destination.MainModule.ForEachType(t =>
                    {

                        logger.WriteLine($"[{tDef.FullName}]|[{t.FullName}]");

                        TypeReference? check;
                        check = t.BaseType;
                        if (check != null && CheckTypeShouldUpdate(ref check, tDef))
                        {
                            t.BaseType = check;
                        }
                        for (int i = 0; i < t.Interfaces.Count; i++)
                        {
                            check = t.Interfaces[i].InterfaceType;
                            if (CheckTypeShouldUpdate(ref check, tRef))
                            {
                                t.Interfaces[i].InterfaceType = check;
                            }
                        }
                        foreach (var e in t.Events)
                        {
                            check = e.EventType;
                            if (CheckTypeShouldUpdate(ref check, tRef))
                            {
                                e.EventType = check;
                            }
                        }
                        foreach (var f in t.Fields)
                        {
                            check = f.FieldType;
                            if (CheckTypeShouldUpdate(ref check, tRef))
                            {
                                f.FieldType = check;
                            }
                        }
                        foreach (var p in t.Properties)
                        {
                            check = p.PropertyType;
                            if (CheckTypeShouldUpdate(ref check, tRef))
                            {
                                p.PropertyType = check;
                            }
                            foreach (var pp in p.Parameters)
                            {
                                check = pp.ParameterType;
                                if (CheckTypeShouldUpdate(ref check, tRef))
                                {
                                    pp.ParameterType = check;
                                }
                            }
                        }
                        foreach (var m in t.Methods.Concat(t.Methods.SelectMany(m => m.Overrides)))
                        {
                            check = m.MethodReturnType.ReturnType;
                            if (CheckTypeShouldUpdate(ref check, tRef))
                            {
                                m.MethodReturnType.ReturnType = check;
                            }
                            check = m.ReturnType;
                            if (CheckTypeShouldUpdate(ref check, tRef))
                            {
                                m.ReturnType = check;
                            }
                            foreach (var p in m.Parameters)
                            {
                                check = p.ParameterType;
                                if (CheckTypeShouldUpdate(ref check, tRef))
                                {
                                    p.ParameterType = check;
                                }
                            }
                            if (m is MethodDefinition md && md.Body != null)
                            {
                                foreach (var v in md.Body.Variables)
                                {
                                    check = v.VariableType;
                                    if (CheckTypeShouldUpdate(ref check, tRef))
                                    {
                                        v.VariableType = check;
                                    }
                                }
                                foreach (var il in md.Body.Instructions)
                                {
                                    if (il.Operand is TypeReference oldTypeRef)
                                    {
                                        check = oldTypeRef;
                                        if (CheckTypeShouldUpdate(ref check, tRef))
                                        {
                                            il.Operand = check;
                                        }
                                    }
                                    else if (il.Operand is MemberReference oldMRef)
                                    {
                                        if (oldMRef.DeclaringType?.FullName == tRef.FullName)
                                        {
                                            MemberReference? mRef = null;
                                            switch (oldMRef)
                                            {
                                                case MethodReference mr:
                                                    {
                                                        var mb = tDef.Methods.FirstOrDefault(m =>
                                                        {
                                                            if (m.Name != mr.Name)
                                                            {
                                                                return false;
                                                            }
                                                            var ps = m.Parameters;
                                                            if (mr.Parameters.Count != ps.Count)
                                                            {
                                                                return false;
                                                            }
                                                            for (int i = 0; i < ps.Count; i++)
                                                            {
                                                                if (mr.Parameters[i].ParameterType.Name != ps[i].ParameterType.Name)
                                                                {
                                                                    return false;
                                                                }
                                                            }
                                                            return true;
                                                        });
                                                        MethodReference nmr;
                                                        if (mr is GenericInstanceMethod gm)
                                                        {
                                                            for (int i = 0; i < gm.GenericArguments.Count; i++)
                                                            {
                                                                check = gm.GenericArguments[i];
                                                                if (CheckTypeShouldUpdate(ref check, tRef))
                                                                {
                                                                    gm.GenericArguments[i] = check;
                                                                }
                                                            }
                                                            nmr = destination.MainModule.ImportReference(mb ?? throw new Exception(), gm);
                                                        }
                                                        else
                                                        {
                                                            nmr = destination.MainModule.ImportReference(mb ?? throw new Exception());
                                                        }

                                                        mRef = nmr;
                                                        break;
                                                    }
                                                case FieldReference fr:
                                                    {
                                                        mRef = destination.MainModule.ImportReference(tDef.FindField(fr.Name));
                                                        break;
                                                    }
                                            }
                                            il.Operand = mRef ?? throw new Exception();
                                        }
                                        else
                                        {
                                            check = oldMRef.DeclaringType;
                                            if (check != null && CheckTypeShouldUpdate(ref check, tRef))
                                            {
                                                oldMRef.DeclaringType = check;
                                            }

                                            switch (oldMRef)
                                            {
                                                case FieldReference fr:
                                                    var ft = fr.FieldType;
                                                    if (CheckTypeShouldUpdate(ref ft, tRef))
                                                    {
                                                        fr.FieldType = ft;
                                                    }
                                                    break;
                                                case PropertyReference pr:
                                                    var pt = pr.PropertyType;
                                                    if (CheckTypeShouldUpdate(ref pt, tRef))
                                                    {
                                                        pr.PropertyType = pt;
                                                    }
                                                    foreach (var p in pr.Parameters)
                                                    {
                                                        var ppt = p.ParameterType;
                                                        if (CheckTypeShouldUpdate(ref ppt, tRef))
                                                        {
                                                            p.ParameterType = ppt;
                                                        }
                                                    }
                                                    break;
                                                case MethodReference mr:
                                                    var rt = mr.ReturnType;
                                                    if (CheckTypeShouldUpdate(ref rt, tRef))
                                                    {
                                                        mr.ReturnType = rt;
                                                    }
                                                    foreach (var gp in mr.GenericParameters)
                                                    {
                                                        foreach (var gc in gp.Constraints)
                                                        {
                                                            var c = gc.ConstraintType;
                                                            if (CheckTypeShouldUpdate(ref c, tRef))
                                                            {
                                                                gc.ConstraintType = c;
                                                            }
                                                        }
                                                    }
                                                    if (mr is GenericInstanceMethod gm)
                                                    {
                                                        for (int i = 0; i < gm.GenericArguments.Count; i++)
                                                        {
                                                            check = gm.GenericArguments[i];
                                                            if (CheckTypeShouldUpdate(ref check, tRef))
                                                            {
                                                                gm.GenericArguments[i] = check;
                                                            }
                                                        }
                                                    }
                                                    break;
                                            }
                                        }
                                        if (il.Operand is MethodReference fmr)
                                        {
                                            check = fmr.MethodReturnType.ReturnType;
                                            if (CheckTypeShouldUpdate(ref check, tRef))
                                            {
                                                fmr.MethodReturnType.ReturnType = check;
                                            }
                                            check = fmr.ReturnType;
                                            if (CheckTypeShouldUpdate(ref check, tRef))
                                            {
                                                fmr.ReturnType = check;
                                            }
                                            foreach (var p in fmr.Parameters)
                                            {
                                                check = p.ParameterType;
                                                if (CheckTypeShouldUpdate(ref check, tRef))
                                                {
                                                    p.ParameterType = check;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    });

                    forwardeds.Add(type);
                }
            }
        }
        static bool CheckTypeShouldUpdate(ref TypeReference checkType, TypeReference newTypeRef)
        {
            var res = false;
            if (checkType.GenericParameters.Count > 0)
            {
                foreach (var gp in checkType.GenericParameters)
                {
                    foreach (var gc in gp.Constraints)
                    {
                        var check = gc.ConstraintType;
                        if (CheckTypeShouldUpdate(ref check, newTypeRef))
                        {
                            gc.ConstraintType = check;
                            res = true;
                        }
                    }
                }
            }
            if (checkType.FullName == newTypeRef.FullName)
            {
                checkType = newTypeRef;
                return true;
            }
            //'ElementType' of 'FunctionPointerType' is null
            else if (checkType is FunctionPointerType fpt)
            {
                var check = fpt.MethodReturnType.ReturnType;
                if (CheckTypeShouldUpdate(ref check, newTypeRef))
                {
                    fpt.MethodReturnType.ReturnType = check;
                    res = true;
                }
                check = fpt.ReturnType;
                if (CheckTypeShouldUpdate(ref check, newTypeRef))
                {
                    fpt.ReturnType = check;
                    res = true;
                }
                foreach (var p in fpt.Parameters)
                {
                    check = p.ParameterType;
                    if (CheckTypeShouldUpdate(ref check, newTypeRef))
                    {
                        p.ParameterType = check;
                        res = true;
                    }
                }
                return res;
            }
            else if (checkType is TypeSpecification tst)
            {
                if (tst is GenericInstanceType gInsType)
                {
                    for (int i = 0; i < gInsType.GenericArguments.Count; i++)
                    {
                        var check = gInsType.GenericArguments[i];
                        if (CheckTypeShouldUpdate(ref check, newTypeRef))
                        {
                            gInsType.GenericArguments[i] = check;
                            res = true;
                        }
                    }
                }
                TypeReference tEle = tst.ElementType;
                if (CheckTypeShouldUpdate(ref tEle, newTypeRef))
                {
                    switch (checkType)
                    {
                        case ArrayType at:
                            {
                                checkType = new ArrayType(tEle, at.Rank);
                                return true;
                            }
                        case OptionalModifierType omt:
                            {
                                var check = omt.ModifierType;
                                CheckTypeShouldUpdate(ref checkType, newTypeRef);
                                checkType = new OptionalModifierType(check, tEle);
                                return true;
                            }
                        case RequiredModifierType rmt:
                            {
                                var check = rmt.ModifierType;
                                CheckTypeShouldUpdate(ref checkType, newTypeRef);
                                checkType = new RequiredModifierType(check, tEle);
                                return true;
                            }
                        case ByReferenceType:
                            {
                                checkType = new ByReferenceType(tEle);
                                return true;
                            }
                        case PinnedType:
                            {
                                checkType = new PinnedType(tEle);
                                return true;
                            }
                        case PointerType:
                            {
                                checkType = new PointerType(tEle);
                                return true;
                            }
                        case SentinelType:
                            {
                                checkType = new SentinelType(tEle);
                                return true;
                            }
                        case GenericInstanceType:
                            {
                                gInsType = (GenericInstanceType)tst;
                                var nGInsT = new GenericInstanceType(tEle);
                                nGInsT.GenericArguments.AddRange(gInsType.GenericArguments);
                                checkType = nGInsT;
                                return true;
                            }
                    }
                }
            }
            return res;
        }
    }
}
