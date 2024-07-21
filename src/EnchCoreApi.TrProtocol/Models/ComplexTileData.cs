using EnchCoreApi.TrProtocol.Attributes;
using EnchCoreApi.TrProtocol.Interfaces;
using System.Runtime.CompilerServices;

namespace EnchCoreApi.TrProtocol.Models {
    public partial struct ComplexTileData : IRepeatElement<short>
    {
        public ComplexTileFlags1 Flags1;
        public ComplexTileFlags2 Flags2;
        public ComplexTileFlags3 Flags3;
        public ComplexTileFlags4 Flags4;

        public ushort TileType;
        public short FrameX;
        public short FrameY;
        public byte TileColor;
        public ushort WallType;
        public byte WallColor;
        public byte Liquid;

        public short RepeatCount { get; set; }

        public unsafe void ReadContent(ref void* ptr) {
            var ptr_current = ptr;

            Flags1 = Unsafe.Read<ComplexTileFlags1>(ptr_current);
            ptr_current = Unsafe.Add<ComplexTileFlags1>(ptr_current, 1);
            if (Flags1.HasFlags2) {
                Flags2 = Unsafe.Read<ComplexTileFlags2>(ptr_current);
                ptr_current = Unsafe.Add<ComplexTileFlags2>(ptr_current, 1);
            }
            if (Flags2.HasFlags3) {
                Flags3 = Unsafe.Read<ComplexTileFlags3>(ptr_current);
                ptr_current = Unsafe.Add<ComplexTileFlags3>(ptr_current, 1);
            }
            if (Flags3.HasFlags4) {
                Flags4 = Unsafe.Read<ComplexTileFlags4>(ptr_current);
                ptr_current = Unsafe.Add<ComplexTileFlags4>(ptr_current, 1);
            }
            if (Flags1.TileActive) {
                if (Flags1.TileTypeIs2Bytes) {
                    TileType = Unsafe.Read<ushort>(ptr_current);
                    ptr_current = Unsafe.Add<ushort>(ptr_current, 1);
                }
                else {
                    TileType = Unsafe.Read<byte>(ptr_current);
                    ptr_current = Unsafe.Add<byte>(ptr_current, 1);
                }
                if (Constants.tileFrameImportant[TileType]) {
                    FrameX = Unsafe.Read<short>(ptr_current);
                    ptr_current = Unsafe.Add<short>(ptr_current, 1);
                    FrameY = Unsafe.Read<short>(ptr_current);
                    ptr_current = Unsafe.Add<short>(ptr_current, 1);
                }
                if (Flags3.TilePrinted) {
                    TileColor = Unsafe.Read<byte>(ptr_current);
                    ptr_current = Unsafe.Add<byte>(ptr_current, 1);
                }
            }
            if (Flags1.WallActive) {
                WallType = Unsafe.Read<byte>(ptr_current);
                ptr_current = Unsafe.Add<byte>(ptr_current, 1);
                if (Flags3.WallPrinted) {
                    WallColor = Unsafe.Read<byte>(ptr_current);
                    ptr_current = Unsafe.Add<byte>(ptr_current, 1);
                }
            }
            if (Flags1.Liquid != LiquidMode.None) {
                Liquid = Unsafe.Read<byte>(ptr_current);
                ptr_current = Unsafe.Add<byte>(ptr_current, 1);
            }
            if (Flags3.WallTypeIs2Bytes) {
                WallType = (ushort)((Unsafe.Read<byte>(ptr_current) << 8) | WallType);
                ptr_current = Unsafe.Add<byte>(ptr_current, 1);
            }
            if (Flags1.HasSameTile_HighBits) {
                RepeatCount = Unsafe.Read<short>(ptr_current);
                ptr_current = Unsafe.Add<short>(ptr_current, 1);
            }
            else if (Flags1.HasSameTile) {
                RepeatCount = Unsafe.Read<byte>(ptr_current);
                ptr_current = Unsafe.Add<byte>(ptr_current, 1);
            }
            else {
                RepeatCount = 0;
            }

            ptr = ptr_current;
        }

        public unsafe void WriteContent(ref void* ptr) {
            var ptr_current = ptr;

            Unsafe.Write(ptr_current, Flags1);
            ptr_current = Unsafe.Add<ComplexTileFlags1>(ptr_current, 1);
            if (Flags1.HasFlags2) {
                Unsafe.Write(ptr_current, Flags2);
                ptr_current = Unsafe.Add<ComplexTileFlags2>(ptr_current, 1);
            }
            if (Flags2.HasFlags3) {
                Unsafe.Write(ptr_current, Flags3);
                ptr_current = Unsafe.Add<ComplexTileFlags3>(ptr_current, 1);
            }
            if (Flags3.HasFlags4) {
                Unsafe.Write(ptr_current, Flags4);
                ptr_current = Unsafe.Add<ComplexTileFlags4>(ptr_current, 1);
            }
            if (Flags1.TileActive) {
                if (Flags1.TileTypeIs2Bytes) {
                    Unsafe.Write(ptr_current, TileType);
                    ptr_current = Unsafe.Add<ushort>(ptr_current, 1);
                }
                else {
                    Unsafe.Write(ptr_current, (byte)TileType);
                    ptr_current = Unsafe.Add<byte>(ptr_current, 1);
                }
                if (Constants.tileFrameImportant[TileType]) {
                    Unsafe.Write(ptr_current, FrameX);
                    ptr_current = Unsafe.Add<short>(ptr_current, 1);
                    Unsafe.Write(ptr_current, FrameY);
                    ptr_current = Unsafe.Add<short>(ptr_current, 1);
                }
                if (Flags3.TilePrinted) {
                    Unsafe.Write(ptr_current, TileColor);
                    ptr_current = Unsafe.Add<byte>(ptr_current, 1);
                }
            }
            if (Flags1.WallActive) {
                Unsafe.Write(ptr_current, (byte)WallType);
                ptr_current = Unsafe.Add<byte>(ptr_current, 1);
                if (Flags3.WallPrinted) {
                    Unsafe.Write(ptr_current, WallColor);
                    ptr_current = Unsafe.Add<byte>(ptr_current, 1);
                }
            }
            if (Flags1.Liquid != LiquidMode.None) {
                Unsafe.Write(ptr_current, Liquid);
                ptr_current = Unsafe.Add<byte>(ptr_current, 1);
            }
            if (Flags1.WallActive && Flags3.WallTypeIs2Bytes) {
                Unsafe.Write(ptr_current, (byte)(WallType >> 8));
                ptr_current = Unsafe.Add<byte>(ptr_current, 1);
            }
            if (Flags1.HasSameTile_HighBits) {
                Unsafe.Write(ptr_current, RepeatCount);
                ptr_current = Unsafe.Add<short>(ptr_current, 1);
            }
            else if (Flags1.HasSameTile) {
                Unsafe.Write(ptr_current, (byte)RepeatCount);
                ptr_current = Unsafe.Add<byte>(ptr_current, 1);
            }

            ptr = ptr_current;
        }
    }
}
