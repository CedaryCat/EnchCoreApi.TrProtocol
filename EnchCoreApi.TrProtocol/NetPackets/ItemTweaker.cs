using EnchCoreApi.TrProtocol.Attributes;
using EnchCoreApi.TrProtocol.Models.Interfaces;
using Terraria;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class ItemTweaker : NetPacket, IItemSlot {
    public sealed override MessageID Type => MessageID.ItemTweaker;
    public short ItemSlot { get; set; }
    public BitsByte Bit1;
    [Condition(nameof(Bit1), 0)] public uint PackedColor;
    [Condition(nameof(Bit1), 1)] public ushort Damage;
    [Condition(nameof(Bit1), 2)] public float Knockback;
    [Condition(nameof(Bit1), 3)] public ushort UseAnimation;
    [Condition(nameof(Bit1), 4)] public ushort UseTime;
    [Condition(nameof(Bit1), 5)] public short Shoot;
    [Condition(nameof(Bit1), 6)] public float ShootSpeed;
    [Condition(nameof(Bit1), 7)] public BitsByte Bit2;
    [Condition(nameof(Bit2), 0)] public short Width;
    [Condition(nameof(Bit2), 1)] public short Height;
    [Condition(nameof(Bit2), 2)] public float Scale;
    [Condition(nameof(Bit2), 3)] public short Ammo;
    [Condition(nameof(Bit2), 4)] public short UseAmmo;
    [Condition(nameof(Bit2), 4)] public bool NotAmmo;
}