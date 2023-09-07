namespace EnchCoreApi.TrProtocol.Attributes {

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class ConditionNotEqualAttribute : Attribute {
        public readonly string fieldOrProperty;
        public readonly int pred;
        public ConditionNotEqualAttribute(string fieldOrProperty, int pred) {
            this.fieldOrProperty = fieldOrProperty;
            this.pred = pred;
        }
    }
}
