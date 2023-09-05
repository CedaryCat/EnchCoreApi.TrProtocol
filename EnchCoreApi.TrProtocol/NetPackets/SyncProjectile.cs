using EnchCoreApi.TrProtocol.Attributes;
using EnchCoreApi.TrProtocol.Models.Interfaces;
using Microsoft.Xna.Framework;
using Terraria;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class SyncProjectile : NetPacket, IProjSlot, IPlayerSlot {
    public sealed override MessageID Type => MessageID.SyncProjectile;
    public short ProjSlot { get; set; }
    public Vector2 Position;
    public Vector2 Velocity;
    public byte PlayerSlot { get; set; }
    //[Bounds("Terraria238", 955)]
    public short ProjType;
    public BitsByte Bit1;
    [Condition(nameof(Bit1), 2)]
    public BitsByte Bit2;
    [Condition(nameof(Bit1), 0)]
    public float AI1;
    [Condition(nameof(Bit1), 1)]
    public float AI2;
    [Condition(nameof(Bit1), 3)]
    public ushort BannerId;
    [Condition(nameof(Bit1), 4)]
    public short Damange;
    [Condition(nameof(Bit1), 5)]
    public float Knockback;
    [Condition(nameof(Bit1), 6)]
    public short OriginalDamage;
    [Condition(nameof(Bit1), 7)]
    public short UUID;
    [Condition(nameof(Bit2), 0)]
    public float AI3;

}