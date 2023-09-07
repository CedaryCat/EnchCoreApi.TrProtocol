using Terraria.Localization;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class Kick : NetPacket {
    public sealed override MessageID Type => MessageID.Kick;
    public NetworkTextModel Reason;
}
