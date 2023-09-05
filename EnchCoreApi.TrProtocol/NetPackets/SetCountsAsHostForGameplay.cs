using EnchCoreApi.TrProtocol.Models.Interfaces;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class SetCountsAsHostForGameplay : NetPacket, IOtherPlayerSlot {
    public sealed override MessageID Type => MessageID.SetCountsAsHostForGameplay;
    public byte OtherPlayerSlot { get; set; }
    public bool Flag;
}