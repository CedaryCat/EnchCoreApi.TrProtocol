using EnchCoreApi.TrProtocol.Models.Interfaces;
using Terraria;
using Terraria.DataStructures;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class PlayerHurtV2 : NetPacket, IOtherPlayerSlot {
    public sealed override MessageID Type => MessageID.PlayerHurtV2;
    public byte OtherPlayerSlot { get; set; }
    public PlayerDeathReasonModel Reason;
    public short Damage;
    public byte HitDirection;
    public BitsByte Bits1;
    public sbyte CoolDown;
}