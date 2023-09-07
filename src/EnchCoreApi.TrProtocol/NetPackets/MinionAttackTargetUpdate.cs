using EnchCoreApi.TrProtocol.Models.Interfaces;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class MinionAttackTargetUpdate : NetPacket, IPlayerSlot {
    public sealed override MessageID Type => MessageID.MinionAttackTargetUpdate;
    public byte PlayerSlot { get; set; }
    public short MinionAttackTarget;
}