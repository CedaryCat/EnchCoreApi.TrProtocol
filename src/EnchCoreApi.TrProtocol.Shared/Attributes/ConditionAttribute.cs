namespace EnchCoreApi.TrProtocol.Attributes {

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
    public class ConditionAttribute : Attribute {
        public readonly string field;
        public readonly int bit;
        public readonly bool pred;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fieldOrProperty"></param>
        /// <param name="index"></param>
        /// <param name="pred"></param>
        public ConditionAttribute(string fieldOrProperty, byte index, bool pred = true) : this(fieldOrProperty, (int)index, pred) {

        }
        private ConditionAttribute(string fieldOrProperty, int index, bool pred = true) {
            this.bit = index;
            this.field = fieldOrProperty;
            this.pred = pred;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fieldOrProperty"></param>
        /// <param name="pred"></param>
        public ConditionAttribute(string fieldOrProperty, bool pred = true) : this(fieldOrProperty, -1, pred) {
        }
    }
}
