using EnchCoreApi.TrProtocol.Models.Interfaces;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class ResetItemOwner : NetPacket, IItemSlot {
    public sealed override MessageID Type => MessageID.ResetItemOwner;
    public short ItemSlot { get; set; }
}