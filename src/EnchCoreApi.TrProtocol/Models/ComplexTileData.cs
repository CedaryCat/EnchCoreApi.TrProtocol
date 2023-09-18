using EnchCoreApi.TrProtocol.Interfaces;
using Microsoft.VisualBasic;
using System.Runtime.InteropServices;
using Terraria;

namespace EnchCoreApi.TrProtocol.Models {
    public struct ComplexTileData
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

        public short Count;

        public void DeserializeTile(BinaryReader br) {
            this = default;

            //int readedSize = 1;

            Flags1 = br.ReadByte();
            // if HasFlag2 flag is true
            if (Flags1.HasFlags2)
            {
                Flags2 = br.ReadByte();
                //readedSize++;
            }
            if (Flags2.HasFlags3)
            {
                Flags3 = br.ReadByte();
                //readedSize++;
            }
            if (Flags3.HasFlags4)
            {
                Flags4 = br.ReadByte();
                //readedSize++;
            }

            // if HasTile flag is true
            if (Flags1.TileActive)
            {
                // read a byte when this flag is false
                TileType = Flags1.TileTypeIs2Bytes ? br.ReadUInt16() : br.ReadByte();

                //readedSize++;
                //if (Flags1[5]) //readedSize++;

                if (Constants.tileFrameImportant[TileType]) {
                    FrameX = br.ReadInt16();
                    FrameY = br.ReadInt16();

                    //readedSize += 4;
                }

                // if HasTileColor flag is true
                if (Flags3.TilePrinted) {
                    TileColor = br.ReadByte();
                    //readedSize++;
                }
            }

            // if HasWall flag is true
            if (Flags1.WallActive) {
                //readedSize++;
                WallType = br.ReadByte();
                // if HasWallColor flag is true
                if (Flags3.WallPrinted) {
                    //readedSize++;
                    WallColor = br.ReadByte();
                }
            }

            // if Liquid1 or Liquid2 flag is true
            if (Flags1.Liquid != LiquidMode.None) {
                //readedSize++;
                Liquid = br.ReadByte();
            }

            // read the additional byte if wall type is big
            if (Flags3.WallTypeIs2Bytes) {
                //readedSize++;
                WallType = (ushort)((br.ReadByte() << 8) | WallType);
            }

            // if HasCountByte or HasCountInt16 flag is true
            if (Flags1.HasSameTile_HighBits) {
                //readedSize += 2;
                Count = br.ReadInt16();
            }
            else if (Flags1.HasSameTile) {
                //readedSize++;
                Count = br.ReadByte();
            }
            else {
                Count = 0;
            }
            //Console.Write($"{Count + 1}/{readedSize},");
        }

        public void SerializeTile(BinaryWriter bw) {

            //int writedSize = 1;
            bw.Write(Flags1);
            // if HasFlag2 flag is true
            if (Flags1.HasFlags2) {
                bw.Write(Flags2);
                //writedSize++;
            }

            // if HasFlag3 flag is true
            if (Flags2.HasFlags3) { 
                bw.Write(Flags3);
                //writedSize++;
            }

            // if HasFlag3 flag is true
            if (Flags3.HasFlags4) {
                bw.Write(Flags4);
                //writedSize++;
            }

            // if HasTile flag is true
            if (Flags1.TileActive) {
                // write a byte when this flag is false
                if (Flags1.TileTypeIs2Bytes) {
                    bw.Write(TileType);
                    //writedSize += 2;
                }
                else {
                    bw.Write((byte)TileType);
                    //writedSize++;
                }


                if (Constants.tileFrameImportant[TileType]) {
                    bw.Write(FrameX);
                    bw.Write(FrameY);
                    //writedSize += 4;
                }

                // if HasTileColor flag is true
                if (Flags3.TilePrinted) {
                    bw.Write(TileColor);
                    //writedSize++;
                }
            }

            // if HasWall flag is true
            if (Flags1.WallActive) {
                bw.Write((byte)WallType);
                //writedSize++;
                // if HasWallColor flag is true
                if (Flags3.WallPrinted) {
                    bw.Write(WallColor);
                    //writedSize++;
                }
            }

            // if Liquid1 or Liquid2 flag is true
            if (Flags1.Liquid != LiquidMode.None) {
                bw.Write(Liquid);
                //writedSize++;
            }

            // write an additional byte if wall type is greater than byte's max
            if (Flags1.WallActive && Flags3.WallTypeIs2Bytes) {
                bw.Write((byte)(WallType >> 8));
                //writedSize++;
            }

            if (Flags1.HasSameTile_HighBits) {
                bw.Write(Count);
                //writedSize += 2;
            }
            else if (Flags1.HasSameTile) {
                bw.Write((byte)Count);
                //writedSize++;
            }

            //Console.Write($"{Count + 1}/{writedSize},");
        }
    }
}
