namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class RequestWorldInfo : NetPacket {
    public sealed override MessageID Type => MessageID.RequestWorldInfo;
}
