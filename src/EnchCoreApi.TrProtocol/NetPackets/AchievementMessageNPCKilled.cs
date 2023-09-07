namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class AchievementMessageNPCKilled : NetPacket {
    public sealed override MessageID Type => MessageID.AchievementMessageNPCKilled;
    public short NPCType;
}
