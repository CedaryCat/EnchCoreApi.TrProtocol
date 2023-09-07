using EnchCoreApi.TrProtocol.Models.Interfaces;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class PlayerStealth : NetPacket, IPlayerSlot {
    public sealed override MessageID Type => MessageID.PlayerStealth;
    public byte PlayerSlot { get; set; }
    public float Stealth;
}