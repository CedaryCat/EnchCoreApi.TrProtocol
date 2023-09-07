namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class RequestPassword : NetPacket {
    public sealed override MessageID Type => MessageID.RequestPassword;
}
