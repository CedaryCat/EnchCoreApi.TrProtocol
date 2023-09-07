using EnchCoreApi.TrProtocol.Interfaces;

namespace EnchCoreApi.TrProtocol.NetPackets;

public sealed partial class SocialHandshake : NetPacket, IExtraData {
    public sealed override MessageID Type => MessageID.SocialHandshake;
}