using EnchCoreApi.TrProtocol.Models.Interfaces;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class SyncChestItem : NetPacket, IChestSlot {
    public sealed override MessageID Type => MessageID.SyncChestItem;
    public short ChestSlot { get; set; }
    public byte ChestItemSlot;
    public short Stack;
    public byte Prefix;
    public short ItemType;
}