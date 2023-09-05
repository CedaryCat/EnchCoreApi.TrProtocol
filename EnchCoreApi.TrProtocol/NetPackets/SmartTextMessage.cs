using Microsoft.Xna.Framework;
using Terraria.Localization;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class SmartTextMessage : NetPacket {
    public sealed override MessageID Type => MessageID.SmartTextMessage;
    public Color Color;
    public NetworkTextModel Text;
    public short Width;
}
