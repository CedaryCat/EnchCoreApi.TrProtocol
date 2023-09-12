using EnchCoreApi.TrProtocol.Models;
using Terraria.DataStructures;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class TileChange : NetPacket {
    public sealed override MessageID Type => MessageID.TileChange;
    public TileEditAction ChangeType;
    public Point16 Position;
    public short TileType;
    public byte Style;
}
