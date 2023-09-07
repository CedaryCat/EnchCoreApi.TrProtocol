namespace EnchCoreApi.TrProtocol.Attributes {
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
    public class ConditionArrayAttribute : Attribute {
        public readonly string field;
        public readonly byte indexStart;
        public readonly bool pred;
        public ConditionArrayAttribute(string fieldOrProperty, byte indexStart, bool pred = true) {
            this.indexStart = indexStart;
            this.field = fieldOrProperty;
            this.pred = pred;
        }
    }
}
