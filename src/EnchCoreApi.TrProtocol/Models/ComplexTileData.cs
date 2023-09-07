using EnchCoreApi.TrProtocol.Interfaces;
using Microsoft.VisualBasic;
using System.Runtime.InteropServices;
using Terraria;

namespace EnchCoreApi.TrProtocol.Models {
    public struct ComplexTileData {
        public BitsByte Flags1;
        public BitsByte Flags2;
        public BitsByte Flags3;
        public BitsByte Flags4;

        public ushort TileType;
        public short FrameX;
        public short FrameY;
        public byte TileColor;
        public ushort WallType;
        public byte WallColor;
        public byte Liquid;

        public short Count;

        public void DeserializeTile(BinaryReader br) {

            Flags1 = br.ReadByte();
            // if HasFlag2 flag is true
            if (Flags1[0])
                Flags2 = br.ReadByte();
            if (Flags2[0])
                Flags3 = br.ReadByte();
            if (Flags3[0])
                Flags4 = br.ReadByte();

            // if HasTile flag is true
            if (Flags1[1]) {
                // read a byte when this flag is false
                TileType = Flags1[5] ? br.ReadUInt16() : br.ReadByte();
                if (Constants.tileFrameImportant[TileType]) {
                    FrameX = br.ReadInt16();
                    FrameY = br.ReadInt16();
                }

                // if HasTileColor flag is true
                if (Flags3[3])
                    TileColor = br.ReadByte();
            }

            // if HasWall flag is true
            if (Flags1[2]) {
                WallType = br.ReadByte();
                // if HasWallColor flag is true
                if (Flags3[4])
                    WallColor = br.ReadByte();
            }

            // if Liquid1 or Liquid2 flag is true
            if (Flags1[3] || Flags1[4])
                Liquid = br.ReadByte();

            // read the additional byte if wall type is big
            if (Flags3[6]) {
                WallType = (ushort)((br.ReadByte() << 8) | WallType);
            }

            // if HasCountByte or HasCountInt16 flag is true
            if (Flags1[6] || Flags1[7]) {
                Count = Flags1[6] ? br.ReadByte() : br.ReadInt16();
            }
        }

        public void SerializeTile(BinaryWriter bw) {

            //Flags1[6] = Count > 1;
            //Flags1[7] = Count > byte.MaxValue;

            bw.Write(Flags1);
            // if HasFlag2 flag is true
            if (Flags1[0]) bw.Write(Flags2);

            // if HasFlag3 flag is true
            if (Flags2[0]) bw.Write(Flags3);

            // if HasFlag3 flag is true
            if (Flags3[0]) bw.Write(Flags4);

            // if HasTile flag is true
            if (Flags1[1]) {
                // write a byte when this flag is false
                if (Flags1[5])
                    bw.Write(TileType);
                else
                    bw.Write((byte)TileType);


                if (Constants.tileFrameImportant[TileType]) {
                    bw.Write(FrameX);
                    bw.Write(FrameY);
                }

                // if HasTileColor flag is true
                if (Flags3[3])
                    bw.Write(TileColor);
            }

            // if HasWall flag is true
            if (Flags1[2]) {
                bw.Write((byte)WallType);
                // if HasWallColor flag is true
                if (Flags3[4])
                    bw.Write(WallColor);
            }

            // if Liquid1 or Liquid2 flag is true
            if (Flags1[3] || Flags1[4])
                bw.Write(Liquid);

            // write an additional byte if wall type is greater than byte's max
            if (Flags3[6]) {
                bw.Write((byte)(WallType >> 8));
            }

            if (Flags1[6] || Flags1[7]) {
                if (Flags1[7])
                    bw.Write(Count);
                else
                    bw.Write((byte)Count);
            }
        }
    }
}
