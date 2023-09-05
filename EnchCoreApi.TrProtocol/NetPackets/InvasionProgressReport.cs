namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class InvasionProgressReport : NetPacket {
    public sealed override MessageID Type => MessageID.InvasionProgressReport;
    public int Progress;
    public int ProgressMax;
    public sbyte Icon;
    public sbyte Wave;
}