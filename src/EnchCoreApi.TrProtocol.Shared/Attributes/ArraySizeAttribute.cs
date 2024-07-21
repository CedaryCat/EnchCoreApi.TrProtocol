namespace EnchCoreApi.TrProtocol.Attributes {
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class ArraySizeAttribute : Attribute {
        private object[] LengthOfEachRank;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="each">dimension</param>
        public ArraySizeAttribute(params object[] each) {
            LengthOfEachRank = each;
        }
    }
    //public class RankSize {
    //    public string Size => ref_size ?? num_size.ToString();
    //    int num_size;
    //    string? ref_size;
    //    RankSize(int size) {
    //        num_size = size;
    //    }
    //    RankSize(string size) {
    //        ref_size = size;
    //    }
    //    public static implicit operator RankSize(int size) => new RankSize(size);
    //    public static implicit operator RankSize(string size) => new RankSize(size);
    //}
}
