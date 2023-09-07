using EnchCoreApi.TrProtocol.Interfaces;
using System.Runtime.InteropServices;

namespace EnchCoreApi.TrProtocol.Models {
    public struct ItemData : IEquatable<ItemData>, IEquatable<ItemDataLayoutSoild>, ISequentialSerializableData<ItemDataLayoutSoild> {
        public short ItemID;
        public byte Prefix;
        public short Stack;

        public ItemDataLayoutSoild SequentialData {
            get => new ItemDataLayoutSoild(ItemID, Prefix, Stack);
            set {
                ItemID = value.ItemID;
                Prefix = value.Prefix;
                Stack = value.Stack;
            }
        }

        public bool Equals(ItemData other) {
            return ItemID == other.ItemID && Prefix == other.Prefix && Stack == other.Stack;
        }

        public bool Equals(ItemDataLayoutSoild other) {
            return ItemID == other.ItemID && Prefix == other.Prefix && Stack == other.Stack;
        }

        public override bool Equals(object? obj) {
            return obj is ItemDataLayoutSoild soild && Equals(soild);
        }

        public override int GetHashCode() {
            return HashCode.Combine(ItemID, Prefix, Stack);
        }

        public static bool operator ==(ItemData left, ItemData right) {
            return left.Equals(right);
        }

        public static bool operator !=(ItemData left, ItemData right) {
            return !(left == right);
        }
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ItemDataLayoutSoild : IEquatable<ItemData>, IEquatable<ItemDataLayoutSoild> {
        public ItemDataLayoutSoild(short itemID, byte prefix, short stack) {
            ItemID = itemID;
            Prefix = prefix;
            Stack = stack;
        }
        public short ItemID;
        public byte Prefix;
        public short Stack;

        public bool Equals(ItemData other) {
            return ItemID == other.ItemID && Prefix == other.Prefix && Stack == other.Stack;
        }

        public bool Equals(ItemDataLayoutSoild other) {
            return ItemID == other.ItemID && Prefix == other.Prefix && Stack == other.Stack;
        }

        public override bool Equals(object? obj) {
            return obj is ItemDataLayoutSoild soild && Equals(soild);
        }

        public override int GetHashCode() {
            return HashCode.Combine(ItemID, Prefix, Stack);
        }

        public static bool operator ==(ItemDataLayoutSoild left, ItemDataLayoutSoild right) {
            return left.Equals(right);
        }

        public static bool operator !=(ItemDataLayoutSoild left, ItemDataLayoutSoild right) {
            return !(left == right);
        }
    }
}
