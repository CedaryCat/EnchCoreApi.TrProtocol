using EnchCoreApi.TrProtocol.Models.Interfaces;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class StrikeNPC : NetPacket, INPCSlot {
    public sealed override MessageID Type => MessageID.StrikeNPC;
    public short NPCSlot { get; set; }
    public short Damage;
    public float Knockback;
    public byte HitDirection;
    public bool Crit;
}