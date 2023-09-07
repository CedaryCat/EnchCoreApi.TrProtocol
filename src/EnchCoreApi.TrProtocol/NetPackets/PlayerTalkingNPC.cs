using EnchCoreApi.TrProtocol.Models.Interfaces;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class PlayerTalkingNPC : NetPacket, IPlayerSlot, INPCSlot {
    public sealed override MessageID Type => MessageID.PlayerTalkingNPC;
    public byte PlayerSlot { get; set; }
    public short NPCSlot { get; set; }
}