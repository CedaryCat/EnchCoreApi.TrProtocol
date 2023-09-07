using EnchCoreApi.TrProtocol.Models.Interfaces;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class SyncLoadout : NetPacket, IPlayerSlot, ILoadOutSlot {
    public sealed override MessageID Type => MessageID.SyncLoadout;
    public byte PlayerSlot { get; set; }
    public byte LoadOutSlot { get; set; }
    public ushort AccessoryVisibility;
}