using EnchCoreApi.TrProtocol.Attributes;
using EnchCoreApi.TrProtocol.Models;
using EnchCoreApi.TrProtocol.Models.Interfaces;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class SendNPCBuffs : NetPacket, INPCSlot {
    public sealed override MessageID Type => MessageID.SendNPCBuffs;
    public short NPCSlot { get; set; }
    [ArraySize(20)]
    public Buff[] Buffs;
}