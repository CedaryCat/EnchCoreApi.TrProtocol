using EnchCoreApi.TrProtocol.Models.Interfaces;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class SyncPlayerChestIndex : NetPacket, IPlayerSlot {
    public sealed override MessageID Type => MessageID.SyncPlayerChestIndex;
    public byte PlayerSlot { get; set; }
    public short ChestIndex;
}