namespace EnchCoreApi.TrProtocol.Interfaces {
    public interface ISequentialSerializableData<TSequentialLayout> where TSequentialLayout : unmanaged {
        TSequentialLayout SequentialData { get; set; }
    }
}
