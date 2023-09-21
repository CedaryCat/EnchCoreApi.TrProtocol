using EnchCoreApi.TrProtocol.Attributes;

namespace EnchCoreApi.TrProtocol.Models {
    public struct ChestData {
        public override string ToString() {
            return $"[{TileX}, {TileY}] {Name}";
        }
        public short ID;
        public short TileX;
        public short TileY;
        [IgnoreSerialize]
        public string? Name;
        public string NameNotNull { 
            get => Name ?? string.Empty;
            set => Name = value;
        }
    }
}
