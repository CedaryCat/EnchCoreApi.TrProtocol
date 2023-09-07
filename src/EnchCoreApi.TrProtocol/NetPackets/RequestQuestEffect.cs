namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class RequestQuestEffect : NetPacket {
    public sealed override MessageID Type => MessageID.RequestQuestEffect;
}