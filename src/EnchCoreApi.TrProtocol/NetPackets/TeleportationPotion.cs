namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class TeleportationPotion : NetPacket {
    public sealed override MessageID Type => MessageID.TeleportationPotion;
    public byte Style;
}