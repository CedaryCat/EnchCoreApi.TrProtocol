using EnchCoreApi.TrProtocol.Models.Interfaces;
using Terraria;
using Terraria.DataStructures;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class PlayerDeathV2 : NetPacket, IPlayerSlot {
    public sealed override MessageID Type => MessageID.PlayerDeathV2;
    public byte PlayerSlot { get; set; }
    public PlayerDeathReasonModel Reason;
    public short Damage;
    public byte HitDirection;
    public BitsByte Bits1;
}