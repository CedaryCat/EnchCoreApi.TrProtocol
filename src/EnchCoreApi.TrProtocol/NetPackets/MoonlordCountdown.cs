namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class MoonlordCountdown : NetPacket {
    public sealed override MessageID Type => MessageID.MoonlordCountdown;
    public int MaxCountdown;
    public int Countdown;
}