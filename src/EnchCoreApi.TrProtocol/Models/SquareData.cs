using EnchCoreApi.TrProtocol.Attributes;
using EnchCoreApi.TrProtocol.Interfaces;

namespace EnchCoreApi.TrProtocol.Models;
public partial class SquareData : IAutoSerializableData {
    public short TilePosX;
    public short TilePosY;
    public byte Width;
    public byte Height;
    public TileChangeType ChangeType;
    [ArraySize(nameof(Width), nameof(Height))]
    public SimpleTileData[,] Tiles;
}
