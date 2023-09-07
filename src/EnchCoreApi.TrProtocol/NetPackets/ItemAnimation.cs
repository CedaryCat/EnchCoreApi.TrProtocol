using EnchCoreApi.TrProtocol.Models.Interfaces;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class ItemAnimation : NetPacket, IPlayerSlot {
    public sealed override MessageID Type => MessageID.ItemAnimation;
    public byte PlayerSlot { get; set; }
    public float Rotation;
    public short Animation;
}