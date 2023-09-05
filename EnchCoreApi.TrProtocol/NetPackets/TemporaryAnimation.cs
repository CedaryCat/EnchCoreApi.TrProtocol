using Terraria.DataStructures;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class TemporaryAnimation : NetPacket {
    public sealed override MessageID Type => MessageID.TemporaryAnimation;
    public short AniType;
    public short TileType;
    public Point16 Position;
}