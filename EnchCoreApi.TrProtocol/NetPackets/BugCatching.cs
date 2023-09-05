using EnchCoreApi.TrProtocol.Models.Interfaces;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class BugCatching : NetPacket, IPlayerSlot, INPCSlot {
    public sealed override MessageID Type => MessageID.BugCatching;
    public short NPCSlot { get; set; }
    public byte PlayerSlot { get; set; }
}