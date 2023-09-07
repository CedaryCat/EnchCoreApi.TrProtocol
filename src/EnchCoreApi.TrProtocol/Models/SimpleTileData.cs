using Terraria;

namespace EnchCoreApi.TrProtocol.Models;
public struct SimpleTileData {
    public BitsByte Flags1;
    public BitsByte Flags2;
    public BitsByte Flags3;
    public byte TileColor;
    public byte WallColor;
    public ushort TileType;
    public short FrameX;
    public short FrameY;
    public ushort WallType;
    public byte Liquid;
    public byte LiquidType;
}
