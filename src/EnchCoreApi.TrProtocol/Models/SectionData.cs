using EnchCoreApi.TrProtocol.Attributes;
using EnchCoreApi.TrProtocol.Interfaces;
using EnchCoreApi.TrProtocol.Models.TileEntities;
using System;
using System.Buffers;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace EnchCoreApi.TrProtocol.Models;

//TODO: refactor 
public partial class SectionData : ISerializableData {
    public int StartX;
    public int StartY;
    public short Width;
    public short Height;
    public ComplexTileData[] Tiles;

    public short ChestCount;
    [ArraySize(nameof(ChestCount))]
    public ChestData[] Chests;

    public short SignCount;
    [ArraySize(nameof(SignCount))]
    public SignData[] Signs;

    public short TileEntityCount;
    [ArraySize(nameof(TileEntityCount))]
    public TileEntity[] TileEntities;

    public unsafe void ReadContent(ref void* ptr) {
        var compressedLen = Unsafe.Read<short>(Unsafe.Subtract<byte>(ptr, 3)) - 3;

        using var st = new UnmanagedMemoryStream((byte*)ptr, compressedLen);
        ptr = Unsafe.Add<byte>(ptr, compressedLen);

        int decompressedLen = 0;
        var arr = ArrayPool<byte>.Shared.Rent(1024 * 32);
        using (var dst = new DeflateStream(st, CompressionMode.Decompress, true)) {
            int rest = arr.Length;
            int readed;
            do {
                readed = dst.Read(arr, decompressedLen, rest);
                rest -= readed;
                decompressedLen += readed;
            }
            while (readed > 0);
        }
        using var br = new BinaryReader(new MemoryStream(arr, 0, decompressedLen));



        StartX = br.ReadInt32();
        StartY = br.ReadInt32();
        Width = br.ReadInt16();
        Height = br.ReadInt16();

        var totalCount = Width * Height;
        var tilesCache = ArrayPool<ComplexTileData>.Shared.Rent(totalCount);

        Console.WriteLine();
        int tileCurrentCount = 0;
        while (totalCount > 0) {
            tilesCache[tileCurrentCount].DeserializeTile(br);
            totalCount -= tilesCache[tileCurrentCount].Count + 1;
            tileCurrentCount++;
        }
        Console.WriteLine();
        Tiles = new ComplexTileData[tileCurrentCount];
        Array.Copy(tilesCache, Tiles, tileCurrentCount);

        ArrayPool<ComplexTileData>.Shared.Return(tilesCache);

        ChestCount = br.ReadInt16();
        if (ChestCount > 8000)
            throw new Exception("Too many chests!");
        Chests = new ChestData[ChestCount];
        for (int i = 0; i < ChestCount; i++) {
            ref var chest = ref Chests[i];
            chest.ID = br.ReadInt16();
            chest.TileX = br.ReadInt16();
            chest.TileY = br.ReadInt16();
            chest.Name = br.ReadString();
        }

        SignCount = br.ReadInt16();
        if (SignCount > 1000)
            throw new Exception("Too many signs!");
        Signs = new SignData[SignCount];
        for (int i = 0; i < SignCount; i++) {
            ref var sign = ref Signs[i];
            sign.ID = br.ReadInt16();
            sign.TileX = br.ReadInt16();
            sign.TileY = br.ReadInt16();
            sign.Text = br.ReadString();
        }

        TileEntityCount = br.ReadInt16();
        if (TileEntityCount > 1000)
            throw new Exception("Too many tile entities!");

        fixed (void* p = arr) {
            var ptr_entity = Unsafe.Add<byte>(p, (int)br.BaseStream.Position);
            TileEntities = new TileEntity[TileEntityCount];
            for (int i = 0; i < TileEntityCount; i++) {
                TileEntities[i] = TileEntity.ReadTileEntity(ref ptr_entity);
            }
        }



        ArrayPool<byte>.Shared.Return(arr);
    }

    public unsafe void WriteContent(ref void* ptr) {
        var buffer = ArrayPool<byte>.Shared.Rent(1024 * 32);
        using var bw = new BinaryWriter(new MemoryStream(buffer));

        bw.Write(StartX);
        bw.Write(StartY);
        bw.Write(Width);
        bw.Write(Height);
        for (int i = 0; i < Tiles.Length; i++) {
            Tiles[i].SerializeTile(bw);
        }

        bw.Write(ChestCount);
        for (int i = 0; i < ChestCount; i++) {
            var chest = Chests[i];
            bw.Write(chest.ID);
            bw.Write(chest.TileX);
            bw.Write(chest.TileY);
            bw.Write(chest.Name ?? "");
        }

        bw.Write(SignCount);
        for (int i = 0; i < SignCount; i++) {
            var sign = Signs[i];
            bw.Write(sign.ID);
            bw.Write(sign.TileX);
            bw.Write(sign.TileY);
            bw.Write(sign.Text ?? "");
        }

        bw.Write(TileEntityCount);
        if (TileEntityCount > 0) {
            fixed (void* p = buffer) {
                var ptr_entity = Unsafe.Add<byte>(p, (int)bw.BaseStream.Position);
                for (int i = 0; i < TileEntityCount; i++) {
                    TileEntities[i].WriteContent(ref ptr_entity);
                }
                bw.BaseStream.Position = ((long)ptr_entity - (long)p);
            }
        }


        using var ust = new UnmanagedMemoryStream((byte*)ptr, 1024 * 32, 1024 * 32, FileAccess.Write);
        var dst = new DeflateStream(ust, CompressionLevel.SmallestSize, leaveOpen: true);
        dst.Write(buffer, 0, (int)bw.BaseStream.Position);
        dst.Dispose();
        ptr = ust.PositionPointer;
        ArrayPool<byte>.Shared.Return(buffer);
    }
}
