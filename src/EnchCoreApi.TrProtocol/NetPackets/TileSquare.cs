using EnchCoreApi.TrProtocol.Models;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class TileSquare : NetPacket {
    public sealed override MessageID Type => MessageID.TileSquare;
    public SquareData Data;
}
