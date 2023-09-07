using EnchCoreApi.TrProtocol.Models.Interfaces;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class Assorted1 : NetPacket, IPlayerSlot {
    public sealed override MessageID Type => MessageID.Assorted1;
    public byte PlayerSlot { get; set; }
    public byte Unknown;
}