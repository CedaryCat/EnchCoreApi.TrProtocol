namespace EnchCoreApi.TrProtocol.Models {
    public partial struct SignData {
        public override string ToString() {
            return $"[{TileX}, {TileY}] {Text}";
        }
        public short ID;
        public short TileX;
        public short TileY;
        public string Text;
    }
}
