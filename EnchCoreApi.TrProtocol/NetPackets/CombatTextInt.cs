using Microsoft.Xna.Framework;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class CombatTextInt : NetPacket {
    public sealed override MessageID Type => MessageID.CombatTextInt;
    public Vector2 Position;
    public Color Color;
    public int Amount;
}