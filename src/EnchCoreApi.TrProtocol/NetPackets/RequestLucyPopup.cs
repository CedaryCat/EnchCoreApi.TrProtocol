using Microsoft.Xna.Framework;
using Terraria.GameContent;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class RequestLucyPopup : NetPacket {
    public sealed override MessageID Type => MessageID.RequestLucyPopup;
    public LucyAxeMessage_MessageSource Source;
    public byte Variation;
    public Vector2 Velocity;
    public Point Position;
}