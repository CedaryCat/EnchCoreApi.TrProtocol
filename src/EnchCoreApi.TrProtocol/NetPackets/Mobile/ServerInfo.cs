namespace EnchCoreApi.TrProtocol.NetPackets.Mobile {
    public partial class ServerInfo : NetPacket {
        public sealed override MessageID Type => MessageID.ServerInfo;
        public int ListenPort;
        public string WorldName;
        public int MaxTilesX;
        public bool IsCrimson;
        public byte GameMode;
        public byte maxNetPlayers;
    }
}
