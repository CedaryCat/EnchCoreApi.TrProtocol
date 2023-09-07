using Terraria.DataStructures;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class TileEntityPlacement : NetPacket {
    public sealed override MessageID Type => MessageID.TileEntityPlacement;
    public Point16 Position;
    public byte TileEntityType;
}