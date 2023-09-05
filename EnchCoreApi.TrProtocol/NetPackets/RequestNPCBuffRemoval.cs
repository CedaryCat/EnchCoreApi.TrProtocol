using EnchCoreApi.TrProtocol.Models.Interfaces;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class RequestNPCBuffRemoval : NetPacket, INPCSlot {
    public sealed override MessageID Type => MessageID.RequestNPCBuffRemoval;
    public short NPCSlot { get; set; }
    public ushort BuffType;
}