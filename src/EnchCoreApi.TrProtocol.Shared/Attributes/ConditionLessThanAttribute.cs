namespace EnchCoreApi.TrProtocol.Attributes {

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class ConditionLessThanAttribute : Attribute {
        public readonly string fieldOrProperty;
        public readonly int check;
        public ConditionLessThanAttribute(string fieldOrProperty, int check) {
            this.fieldOrProperty = fieldOrProperty;
            this.check = check;
        }
    }
    public class ConditionLessThanEqualAttribute : Attribute {
        public readonly string fieldOrProperty;
        public readonly int check;
        public ConditionLessThanEqualAttribute(string fieldOrProperty, int check) {
            this.fieldOrProperty = fieldOrProperty;
            this.check = check;
        }
    }
}
