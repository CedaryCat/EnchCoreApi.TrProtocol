namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class PoofOfSmoke : NetPacket {
    public sealed override MessageID Type => MessageID.PoofOfSmoke;
    public uint PackedHalfVector2;
}