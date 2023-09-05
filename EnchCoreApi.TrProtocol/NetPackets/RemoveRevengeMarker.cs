namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class RemoveRevengeMarker : NetPacket {
    public sealed override MessageID Type => MessageID.RemoveRevengeMarker;
    public int ID;
}