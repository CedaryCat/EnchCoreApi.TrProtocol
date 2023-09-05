using EnchCoreApi.TrProtocol.Models.Interfaces;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class PlayNote : NetPacket, IPlayerSlot {
    public sealed override MessageID Type => MessageID.PlayNote;
    public byte PlayerSlot { get; set; }
    public float Range;
}