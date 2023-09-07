namespace EnchCoreApi.TrProtocol.NetPackets {
    public partial class ClientHello : NetPacket {
        public sealed override MessageID Type => MessageID.ClientHello;
        public string Version;
    }
}
