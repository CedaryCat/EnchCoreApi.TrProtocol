namespace EnchCoreApi.TrProtocol.Interfaces {
    public partial interface ISerializableData {
        public unsafe void ReadContent(ref void* ptr);
        public unsafe void WriteContent(ref void* ptr);
    }
}
