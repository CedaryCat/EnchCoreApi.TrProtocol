namespace EnchCoreApi.TrProtocol.Attributes {

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class ConditionEqualAttribute : Attribute {
        public readonly string fieldOrProperty;
        public readonly int pred;
        public ConditionEqualAttribute(string fieldOrProperty, int pred) {
            this.fieldOrProperty = fieldOrProperty;
            this.pred = pred;
        }
    }
}
