using EnchCoreApi.TrProtocol.Models.Interfaces;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class SpiritHeal : NetPacket, IOtherPlayerSlot {
    public sealed override MessageID Type => MessageID.SpiritHeal;
    public byte OtherPlayerSlot { get; set; }
    public short Amount;
}