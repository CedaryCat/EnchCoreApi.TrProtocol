namespace EnchCoreApi.TrProtocol.Attributes {
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum, AllowMultiple = false, Inherited = false)]
    public class TypeConvertionAttribute : Attribute {
        public readonly string Type;
        public TypeConvertionAttribute(string targetTypeFullName) {
            Type = targetTypeFullName;
        }
    }
}
