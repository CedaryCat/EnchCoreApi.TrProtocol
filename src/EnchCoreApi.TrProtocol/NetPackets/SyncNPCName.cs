
using EnchCoreApi.TrProtocol.Attributes;
using EnchCoreApi.TrProtocol.Interfaces;
using EnchCoreApi.TrProtocol.Models.Interfaces;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class SyncNPCName : NetPacket, INPCSlot, ISideDependent {
    public sealed override MessageID Type => MessageID.SyncNPCName;
    public short NPCSlot { get; set; }
    [S2COnly]
    public string? NPCName;
    [S2COnly]
    public int TownNpc;
}
