namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class ToggleParty : NetPacket {
    public sealed override MessageID Type => MessageID.ToggleParty;
}