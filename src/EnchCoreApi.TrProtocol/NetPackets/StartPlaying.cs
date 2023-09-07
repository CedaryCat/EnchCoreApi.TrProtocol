namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class StartPlaying : NetPacket {
    public sealed override MessageID Type => MessageID.StartPlaying;
}
