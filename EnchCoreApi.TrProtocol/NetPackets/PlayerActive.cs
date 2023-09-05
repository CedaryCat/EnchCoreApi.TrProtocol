using EnchCoreApi.TrProtocol.Models.Interfaces;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class PlayerActive : NetPacket, IPlayerSlot {
    public sealed override MessageID Type => MessageID.PlayerActive;
    public byte PlayerSlot { get; set; }
    public bool Active;
}
