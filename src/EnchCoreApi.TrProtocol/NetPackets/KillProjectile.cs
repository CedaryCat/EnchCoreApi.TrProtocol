using EnchCoreApi.TrProtocol.Models.Interfaces;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class KillProjectile : NetPacket, IProjSlot, IPlayerSlot {
    public sealed override MessageID Type => MessageID.KillProjectile;
    public short ProjSlot { get; set; }
    public byte PlayerSlot { get; set; }
}