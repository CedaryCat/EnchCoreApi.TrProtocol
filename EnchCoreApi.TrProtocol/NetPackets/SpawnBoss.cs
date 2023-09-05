using EnchCoreApi.TrProtocol.Models.Interfaces;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class SpawnBoss : NetPacket, IOtherPlayerSlot {
    public sealed override MessageID Type => MessageID.SpawnBoss;
    public byte OtherPlayerSlot { get; set; }
    public byte HighBitOfPlayerIsAlwaysZero = 0;
    public short NPCType;
}