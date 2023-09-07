using EnchCoreApi.TrProtocol.Models.Interfaces;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class ItemOwner : NetPacket, IItemSlot, IOtherPlayerSlot {
    public sealed override MessageID Type => MessageID.ItemOwner;
    public short ItemSlot { get; set; }
    public byte OtherPlayerSlot { get; set; }
}
