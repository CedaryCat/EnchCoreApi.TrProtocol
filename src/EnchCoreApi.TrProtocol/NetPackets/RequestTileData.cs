using Microsoft.Xna.Framework;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class RequestTileData : NetPacket {
    public sealed override MessageID Type => MessageID.RequestTileData;
    public Point Position;
}
