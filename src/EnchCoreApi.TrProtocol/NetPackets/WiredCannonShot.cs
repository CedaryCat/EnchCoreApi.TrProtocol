using EnchCoreApi.TrProtocol.Models.Interfaces;
using Terraria.DataStructures;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class WiredCannonShot : NetPacket, IPlayerSlot {
    public sealed override MessageID Type => MessageID.WiredCannonShot;
    public short Damage;
    public float Knockback;
    public Point16 Position;
    public short Angle;
    public short Ammo;
    public byte PlayerSlot { get; set; }
}