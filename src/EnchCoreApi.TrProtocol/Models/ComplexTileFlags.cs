using EnchCoreApi.TrProtocol.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnchCoreApi.TrProtocol.Models
{
    public struct ComplexTileFlags1 : ISoildSerializableData
    {
        public byte InnerData;
        public bool HasFlags2 {
            readonly get => (InnerData & 1u) == 1u;
            set { if (value) InnerData |= 1; else InnerData = (byte)(InnerData & ~1); }
        }
        public bool TileActive {
            readonly get => (InnerData & 2) == 2;
            set { if (value) InnerData |= 2; else InnerData = (byte)(InnerData & ~2); }
        }
        public bool WallActive {
            readonly get => (InnerData & 4) == 4;
            set { if (value) InnerData |= 4; else InnerData = (byte)(InnerData & ~4); }
        }
        public LiquidMode Liquid {
            get => (LiquidMode)((InnerData & 0b00011000) >> 3);
            set {
                var val = (byte)((byte)value << 3);
                val &= 0b00011000;
                InnerData &= 0b11100111;
                InnerData |= val;
            }
        }
        public bool HasWaterOrShimmer {
            get {
                return (InnerData & 0b00011000) == 0b00001000;
            }
            set {
                InnerData &= 0b11100111;
                if (value) {
                    InnerData |= 0b00001000;
                }
            }
        }
        public bool HasLava {
            get {
                return (InnerData & 0b00011000) == 0b00010000;
            }
            set {
                InnerData &= 0b11100111;
                if (value) {
                    InnerData |= 0b00010000;
                }
            }
        }
        public bool HasHoney {
            get {
                return (InnerData & 0b00011000) == 0b00011000;
            }
            set {
                InnerData &= 0b11100111;
                if (value) {
                    InnerData |= 0b00011000;
                }
            }
        }
        public bool TileTypeIs2Bytes {
            readonly get => (InnerData & 32) == 32;
            set { if (value) InnerData |= 32; else InnerData = (byte)(InnerData & ~32); }
        }
        public bool HasSameTile {
            readonly get => (InnerData & 64) == 64;
            set { if (value) InnerData |= 64; else InnerData = (byte)(InnerData & ~64); }
        }
        public bool HasSameTile_HighBits {
            readonly get => (InnerData & 128) == 128;
            set { if (value) InnerData |= 128; else InnerData = (byte)(InnerData & ~128); }
        }

        public static implicit operator byte(ComplexTileFlags1 flag) {
            return flag.InnerData;
        }

        public static implicit operator ComplexTileFlags1(byte b) {
            ComplexTileFlags1 result = default;
            result.InnerData = b;
            return result;
        }
    }
    public enum LiquidMode : byte
    {
        None = 0, 
        WaterOrShimmer = 0b01,
        Lava = 0b10,
        Honey = 0b11,
    }
    public struct ComplexTileFlags2 : ISoildSerializableData
    {
        public byte InnerData;
        public bool HasFlags3 {
            readonly get => (InnerData & 1u) == 1u;
            set { if (value) InnerData |= 1; else InnerData = (byte)(InnerData & ~1); }
        }
        public bool WireRed {
            readonly get => (InnerData & 2) == 2;
            set { if (value) InnerData |= 2; else InnerData = (byte)(InnerData & ~2); }
        }
        public bool WireBlue {
            readonly get => (InnerData & 4) == 4;
            set { if (value) InnerData |= 4; else InnerData = (byte)(InnerData & ~4); }
        }
        public bool WireYellow {
            readonly get => (InnerData & 4) == 4;
            set { if (value) InnerData |= 4; else InnerData = (byte)(InnerData & ~4); }
        }
        public TileSolpeMode TileSolpeMode {
            get => (TileSolpeMode)((InnerData & 0b01110000) >> 4);
            set {
                var val = (byte)((byte)value << 4);
                val &= 0b01110000;
                InnerData &= 0b10001111;
                InnerData |= val;
            }
        }

        public static implicit operator byte(ComplexTileFlags2 flag) {
            return flag.InnerData;
        }

        public static implicit operator ComplexTileFlags2(byte b) {
            ComplexTileFlags2 result = default;
            result.InnerData = b;
            return result;
        }
    }
    public enum TileSolpeMode {
        None = 0,
        HalfBrick = 1,
    }
    public struct ComplexTileFlags3 : ISoildSerializableData
    {
        public byte InnerData;
        public bool HasFlags4 {
            readonly get => (InnerData & 1u) == 1u;
            set { if (value) InnerData |= 1; else InnerData = (byte)(InnerData & ~1); }
        }
        public bool Actuator {
            readonly get => (InnerData & 2) == 2;
            set { if (value) InnerData |= 2; else InnerData = (byte)(InnerData & ~2); }
        }
        public bool InActive {
            readonly get => (InnerData & 4) == 4;
            set { if (value) InnerData |= 4; else InnerData = (byte)(InnerData & ~4); }
        }
        public bool TilePrinted {
            readonly get => (InnerData & 8) == 8;
            set { if (value) InnerData |= 4; else InnerData = (byte)(InnerData & ~8); }
        }
        public bool WallPrinted {
            readonly get => (InnerData & 16) == 16;
            set { if (value) InnerData |= 16; else InnerData = (byte)(InnerData & ~16); }
        }
        public bool HasWire4 {
            readonly get => (InnerData & 32) == 32;
            set { if (value) InnerData |= 32; else InnerData = (byte)(InnerData & ~32); }
        }
        public bool WallTypeIs2Bytes {
            readonly get => (InnerData & 64) == 64;
            set { if (value) InnerData |= 64; else InnerData = (byte)(InnerData & ~64); }
        }
        public bool ShimmerOverrideWater {
            readonly get => (InnerData & 128) == 128;
            set { if (value) InnerData |= 128; else InnerData = (byte)(InnerData & ~128); }
        }

        public static implicit operator byte(ComplexTileFlags3 flag) {
            return flag.InnerData;
        }

        public static implicit operator ComplexTileFlags3(byte b) {
            ComplexTileFlags3 result = default;
            result.InnerData = b;
            return result;
        }
    }
    public struct ComplexTileFlags4 : ISoildSerializableData
    {
        public byte InnerData;
        public bool InvisibleBlock {
            readonly get => (InnerData & 2) == 2;
            set { if (value) InnerData |= 2; else InnerData = (byte)(InnerData & ~2); }
        }
        public bool InvisibleWall {
            readonly get => (InnerData & 4) == 4;
            set { if (value) InnerData |= 4; else InnerData = (byte)(InnerData & ~4); }
        }
        public bool FullbrightBlock {
            readonly get => (InnerData & 8) == 8;
            set { if (value) InnerData |= 8; else InnerData = (byte)(InnerData & ~8); }
        }
        public bool FullbrightWall {
            readonly get => (InnerData & 16) == 16;
            set { if (value) InnerData |= 16; else InnerData = (byte)(InnerData & ~16); }
        }

        public static implicit operator byte(ComplexTileFlags4 flag) {
            return flag.InnerData;
        }

        public static implicit operator ComplexTileFlags4(byte b) {
            ComplexTileFlags4 result = default;
            result.InnerData = b;
            return result;
        }
    }
}
