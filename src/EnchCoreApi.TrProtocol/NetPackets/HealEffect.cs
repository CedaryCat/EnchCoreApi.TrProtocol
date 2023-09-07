using EnchCoreApi.TrProtocol.Models.Interfaces;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class HealEffect : NetPacket, IPlayerSlot {
    public sealed override MessageID Type => MessageID.HealEffect;
    public byte PlayerSlot { get; set; }
    public short Amount;
}