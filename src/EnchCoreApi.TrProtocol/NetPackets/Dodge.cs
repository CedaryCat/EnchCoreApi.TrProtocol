using EnchCoreApi.TrProtocol.Models.Interfaces;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class Dodge : NetPacket, IPlayerSlot {
    public sealed override MessageID Type => MessageID.Dodge;
    public byte PlayerSlot { get; set; }
    public byte DodgeType;
}