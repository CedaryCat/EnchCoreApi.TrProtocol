namespace EnchCoreApi.TrProtocol.Attributes {
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class ConvertionOptionAttribute : Attribute {
        public readonly ConvertionOption Option;

        public ConvertionOptionAttribute(ConvertionOption option = ConvertionOption.Copy) {
            Option = option;
        }
    }
    public enum ConvertionOption {
        Copy,
        Ignore,
    }
}
