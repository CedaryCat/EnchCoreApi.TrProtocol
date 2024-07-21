using EnchCoreApi.TrProtocol.Attributes;
using EnchCoreApi.TrProtocol.Models.Interfaces;
using Terraria;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class ItemTweaker : NetPacket, IItemSlot {
    public sealed override MessageID Type => MessageID.ItemTweaker;
    public short ItemSlot { get; set; }
    public BitsByte Bit1;
    [InitDefaultValue]
    [Condition(nameof(Bit1), 0)] public uint PackedColor;
    [InitDefaultValue]
    [Condition(nameof(Bit1), 1)] public ushort Damage;
    [InitDefaultValue]
    [Condition(nameof(Bit1), 2)] public float Knockback;
    [InitDefaultValue]
    [Condition(nameof(Bit1), 3)] public ushort UseAnimation;
    [InitDefaultValue]
    [Condition(nameof(Bit1), 4)] public ushort UseTime;
    [InitDefaultValue]
    [Condition(nameof(Bit1), 5)] public short Shoot;
    [InitDefaultValue]
    [Condition(nameof(Bit1), 6)] public float ShootSpeed;
    [Condition(nameof(Bit1), 7)] public BitsByte Bit2;
    [InitDefaultValue]
    [Condition(nameof(Bit2), 0)] public short Width;
    [InitDefaultValue]
    [Condition(nameof(Bit2), 1)] public short Height;
    [InitDefaultValue]
    [Condition(nameof(Bit2), 2)] public float Scale;
    [InitDefaultValue]
    [Condition(nameof(Bit2), 3)] public short Ammo;
    [InitDefaultValue]
    [Condition(nameof(Bit2), 4)] public short UseAmmo;
    [InitDefaultValue]
    [Condition(nameof(Bit2), 4)] public bool NotAmmo;
}