using EnchCoreApi.TrProtocol.Interfaces;

namespace EnchCoreApi.TrProtocol.NetPackets;

public sealed partial class SyncEmoteBubble : NetPacket, IExtraData {
    public sealed override MessageID Type => MessageID.SyncEmoteBubble;
    public int ID;
    public byte EmoteType;
}