using EnchCoreApi.TrProtocol.Models.Interfaces;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class AddNPCBuff : NetPacket, INPCSlot {
    public sealed override MessageID Type => MessageID.AddNPCBuff;
    public short NPCSlot { get; set; }
    public ushort BuffType;
    public short BuffTime;
}
