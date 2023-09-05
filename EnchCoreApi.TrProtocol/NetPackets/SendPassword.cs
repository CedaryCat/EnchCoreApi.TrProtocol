namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class SendPassword : NetPacket {
    public sealed override MessageID Type => MessageID.SendPassword;
    public string Password;
}
