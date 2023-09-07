namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class LiquidUpdate : NetPacket {
    public sealed override MessageID Type => MessageID.LiquidUpdate;
    public short TileX;
    public short TileY;
    public byte Liquid;
    public byte LiquidType;
}
