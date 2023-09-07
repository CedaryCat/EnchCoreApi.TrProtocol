using Terraria.DataStructures;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class PaintTile : NetPacket {
    public sealed override MessageID Type => MessageID.PaintTile;
    public Point16 Position;
    public byte Color;
}