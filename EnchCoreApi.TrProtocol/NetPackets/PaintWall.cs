using Terraria.DataStructures;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class PaintWall : NetPacket {
    public sealed override MessageID Type => MessageID.PaintWall;
    public Point16 Position;
    public byte Color;
}