using EnchCoreApi.TrProtocol.Attributes;

namespace EnchCoreApi.TrProtocol.Models {
    public partial struct SignData {
        public override string ToString() {
            return $"[{TileX}, {TileY}] {Text}";
        }
        public short ID;
        public short TileX;
        public short TileY;
        [IgnoreSerialize]
        public string? Text;
        public string TextNotNull {
            get => Text ?? string.Empty;
            set => Text = value;
        }
    }
}
