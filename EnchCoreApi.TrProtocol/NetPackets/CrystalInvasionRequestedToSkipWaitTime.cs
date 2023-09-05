namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class CrystalInvasionRequestedToSkipWaitTime : NetPacket {
    public sealed override MessageID Type => MessageID.CrystalInvasionRequestedToSkipWaitTime;
}