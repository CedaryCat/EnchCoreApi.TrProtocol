using EnchCoreApi.TrProtocol.Attributes;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class SyncCavernMonsterType : NetPacket {
    public sealed override MessageID Type => MessageID.SyncCavernMonsterType;
    [ArraySize(6)]
    public short[] CavenMonsterType;
}