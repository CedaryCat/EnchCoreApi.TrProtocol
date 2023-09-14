using EnchCoreApi.TrProtocol.Attributes;
using Terraria;

namespace EnchCoreApi.TrProtocol.Models;
public struct SimpleTileData {
    public BitsByte Flags1;
    public BitsByte Flags2;
    public BitsByte Flags3;
    [Condition(nameof(Flags2), 2)]
    public byte TileColor;
    [Condition(nameof(Flags2), 3)]
    public byte WallColor;
    [Condition(nameof(Flags1), 0)]
    public ushort TileType;
    public bool FrameXYExist => Constants.tileFrameImportant[TileType];
    [Condition(nameof(Flags1), 0), Condition(nameof(FrameXYExist), true)]
    public short FrameX;
    [Condition(nameof(Flags1), 0), Condition(nameof(FrameXYExist), true)]
    public short FrameY;
    [Condition(nameof(Flags1), 2)]
    public ushort WallType;
    [Condition(nameof(Flags1), 3)]
    public byte Liquid;
    [Condition(nameof(Flags1), 3)]
    public byte LiquidType;
}
