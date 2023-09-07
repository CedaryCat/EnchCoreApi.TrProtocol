using EnchCoreApi.TrProtocol.Models.Interfaces;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class QuickStackChests : NetPacket, IChestSlot {
    public sealed override MessageID Type => MessageID.QuickStackChests;
    public short ChestSlot { get; set; }
}