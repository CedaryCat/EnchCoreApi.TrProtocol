using EnchCoreApi.TrProtocol.Attributes;
using EnchCoreApi.TrProtocol.Models.Interfaces;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class TamperWithNPC : NetPacket, INPCSlot, IOtherPlayerSlot {
    public sealed override MessageID Type => MessageID.TamperWithNPC;
    public short NPCSlot { get; set; }
    public byte UniqueImmune;

    [ConditionEqual(nameof(UniqueImmune), 1)]
    public int Time;
    [ConditionEqual(nameof(UniqueImmune), 1)]
    public byte OtherPlayerSlot { get; set; }
    public byte HighBitOfPlayerIsAlwaysZero = 0;
}