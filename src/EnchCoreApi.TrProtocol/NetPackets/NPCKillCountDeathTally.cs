namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class NPCKillCountDeathTally : NetPacket {
    public sealed override MessageID Type => MessageID.NPCKillCountDeathTally;
    public short NPCType;
    public int Count;
}