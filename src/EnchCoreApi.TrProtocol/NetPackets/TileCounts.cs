namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class TileCounts : NetPacket {
    public sealed override MessageID Type => MessageID.TileCounts;
    public byte Good;
    public byte Evil;
    public byte Blood;
}