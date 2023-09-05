using EnchCoreApi.TrProtocol.Models.Interfaces;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class ManaEffect : NetPacket, IPlayerSlot {
    public sealed override MessageID Type => MessageID.ManaEffect;
    public byte PlayerSlot { get; set; }
    public short Amount;
}