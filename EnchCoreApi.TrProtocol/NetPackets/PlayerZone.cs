using EnchCoreApi.TrProtocol.Attributes;
using EnchCoreApi.TrProtocol.Models.Interfaces;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class PlayerZone : NetPacket, IPlayerSlot {
    public sealed override MessageID Type => MessageID.PlayerZone;
    public byte PlayerSlot { get; set; }
    [ArraySize(5)]
    public byte[] Zone;
}