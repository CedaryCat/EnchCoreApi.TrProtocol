using Microsoft.Xna.Framework;
using Terraria.Localization;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class CombatTextString : NetPacket {
    public sealed override MessageID Type => MessageID.CombatTextString;
    public Vector2 Position;
    public Color Color;
    public NetworkTextModel Text;
}