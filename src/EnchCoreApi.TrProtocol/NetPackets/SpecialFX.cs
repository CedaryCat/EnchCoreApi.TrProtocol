using Microsoft.Xna.Framework;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class SpecialFX : NetPacket {
    public sealed override MessageID Type => MessageID.SpecialFX;
    public byte GrowType;
    public Point Position;
    public byte Height;
    public short Gore;
}