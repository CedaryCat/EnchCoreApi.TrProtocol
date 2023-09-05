namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class ClientUUID : NetPacket {
    public sealed override MessageID Type => MessageID.ClientUUID;
    public string UUID;
}