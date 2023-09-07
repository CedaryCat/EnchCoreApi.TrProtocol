using EnchCoreApi.TrProtocol.Attributes;
using EnchCoreApi.TrProtocol.Interfaces;
using EnchCoreApi.TrProtocol.Models.TileEntities;
using System;
using System.Buffers;
using System.Drawing;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace EnchCoreApi.TrProtocol.Models;

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

        var b = new byte[compressedLen];
        Marshal.Copy((IntPtr)ptr, b, 0, compressedLen);
        using var st = new MemoryStream(b);

        using var dst = new DeflateStream(st, CompressionMode.Decompress, true);
        using var br = new BinaryReader(dst);


        StartX = br.ReadInt32();
        StartY = br.ReadInt32();
        Width = br.ReadInt16();
        Height = br.ReadInt16();

        var totalCount = Width * Height;
        var tilesCache = ArrayPool<ComplexTileData>.Shared.Rent(totalCount);

        int tileCurrentCount = 0;
        while (totalCount > 0) {
            tilesCache[tileCurrentCount] = default;
            tilesCache[tileCurrentCount].DeserializeTile(br);
            totalCount -= tilesCache[tileCurrentCount].Count + 1;
            tileCurrentCount++;
        }
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

        //max size is DisplayDoll which take 91b
        var tileEntityDataCache = ArrayPool<byte>.Shared.Rent(91 * TileEntityCount);
        dst.Read(tileEntityDataCache);

        var tileEntityDataBuffer = new Span<byte>(tileEntityDataCache);
        fixed (void* ptr_entity_origi = tileEntityDataBuffer) {
            var ptr_entity = ptr_entity_origi;
            TileEntities = new TileEntity[TileEntityCount];
            for (int i = 0; i < TileEntityCount; i++) {
                TileEntities[i] = TileEntity.ReadTileEntity(ref ptr_entity);
            }
        }

        

        ArrayPool<byte>.Shared.Return(tileEntityDataCache);
        ptr = Unsafe.Add<byte>(ptr, compressedLen);
    }

    public unsafe void WriteContent(ref void* ptr) {
        using var compressed = new MemoryStream();
        using var dst = new DeflateStream(compressed, CompressionLevel.SmallestSize, true);
        using var bw = new BinaryWriter(dst);

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
            bw.Write(sign.Text);
        }

        bw.Write((short)TileEntities.Length);


        var tileEntityDataCache = ArrayPool<byte>.Shared.Rent(91);
        var tileEntityDataBuffer = new Span<byte>(tileEntityDataCache);

        fixed (void* ptr_entity_origi = tileEntityDataBuffer) {
            var ptr_entity = ptr_entity_origi;
            for (int i = 0; i < TileEntityCount; i++) {
                TileEntities[i].WriteContent(ref ptr_entity);
                dst.Write(tileEntityDataCache, 0, (int)((long)ptr_entity - (long)ptr_entity_origi));
            }
        }

        ArrayPool<byte>.Shared.Return(tileEntityDataCache);

        using var ust = new UnmanagedMemoryStream((byte*)ptr, compressed.Position, compressed.Position, FileAccess.Write);
        compressed.CopyTo(ust);
        ptr = ust.PositionPointer;
    }
}
