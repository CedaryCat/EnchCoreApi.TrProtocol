namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class AchievementMessageEventHappened : NetPacket {
    public sealed override MessageID Type => MessageID.AchievementMessageEventHappened;
    public short EventType;
}
