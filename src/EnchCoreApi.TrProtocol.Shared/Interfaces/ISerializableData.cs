namespace EnchCoreApi.TrProtocol.Interfaces {
    public partial interface ISerializableData {
        unsafe void ReadContent(ref void* ptr);
        unsafe void WriteContent(ref void* ptr);
    }
}
