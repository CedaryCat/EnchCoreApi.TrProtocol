namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class MenuSunMoon : NetPacket {
    public sealed override MessageID Type => MessageID.MenuSunMoon;
    public bool DayTime;
    public int Time;
    public short Sun;
    public short Moon;
}
