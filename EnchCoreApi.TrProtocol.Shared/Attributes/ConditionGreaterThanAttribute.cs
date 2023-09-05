namespace EnchCoreApi.TrProtocol.Attributes {

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class ConditionGreaterThanAttribute : Attribute {
        public readonly string fieldOrProperty;
        public readonly int check;
        public ConditionGreaterThanAttribute(string fieldOrProperty, int check) {
            this.fieldOrProperty = fieldOrProperty;
            this.check = check;
        }
    }
    public class ConditionGreaterThanEqualAttribute : Attribute {
        public readonly string fieldOrProperty;
        public readonly int check;
        public ConditionGreaterThanEqualAttribute(string fieldOrProperty, int check) {
            this.fieldOrProperty = fieldOrProperty;
            this.check = check;
        }
    }
}
