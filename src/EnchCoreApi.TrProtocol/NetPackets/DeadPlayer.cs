using EnchCoreApi.TrProtocol.Models.Interfaces;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class DeadPlayer : NetPacket, IOtherPlayerSlot {
    public sealed override MessageID Type => MessageID.DeadPlayer;
    public byte OtherPlayerSlot { get; set; }
}