namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class FinishedConnectingToServer : NetPacket {
    public sealed override MessageID Type => MessageID.FinishedConnectingToServer;
}