using EnchCoreApi.TrProtocol.Models.Interfaces;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class PlayerPvP : NetPacket, IPlayerSlot {
    public sealed override MessageID Type => MessageID.PlayerPvP;
    public byte PlayerSlot { get; set; }
    public bool Pvp;
}