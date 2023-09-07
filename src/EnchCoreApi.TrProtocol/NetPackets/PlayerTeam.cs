using EnchCoreApi.TrProtocol.Models.Interfaces;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class PlayerTeam : NetPacket, IPlayerSlot {
    public sealed override MessageID Type => MessageID.PlayerTeam;
    public byte PlayerSlot { get; set; }
    public byte Team;
}