using EnchCoreApi.TrProtocol.Models.Interfaces;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class RequestTileEntityInteraction : NetPacket, IPlayerSlot {
    public sealed override MessageID Type => MessageID.RequestTileEntityInteraction;
    public int TileEntityID;
    public byte PlayerSlot { get; set; }
}