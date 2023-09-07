namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class CrystalInvasionSendWaitTime : NetPacket {
    public sealed override MessageID Type => MessageID.CrystalInvasionSendWaitTime;
    public int WaitTime;
}