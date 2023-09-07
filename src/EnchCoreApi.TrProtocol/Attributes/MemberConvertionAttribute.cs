namespace EnchCoreApi.TrProtocol.Attributes {
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class MemberConvertionAttribute : Attribute {
        public readonly ConvertionOption Option;
        public string? CustomConvertionFromMethod;
        public string? CustomConvertionToMethod;

        public MemberConvertionAttribute(ConvertionOption option = ConvertionOption.Copy) {
            Option = option;
        }
    }
    public enum ConvertionOption {
        Copy,
        Ignore,
        Custom,
    }
}
