using EnchCoreApi.TrProtocol.Attributes;
using EnchCoreApi.TrProtocol.Interfaces;
using Microsoft.Xna.Framework;
using Terraria;

namespace EnchCoreApi.TrProtocol.NetPackets;

public sealed partial class SyncNPC : NetPacket, IExtraData {
    public sealed override MessageID Type => MessageID.SyncNPC;
    public short NPCSlot;
    public Vector2 Offset;
    [InitDefaultValue]
    public Vector2 Velocity;
    [InitDefaultValue]
    public ushort Target;
    [InitDefaultValue]
    public BitsByte Bit1;
    [InitDefaultValue]
    public BitsByte Bit2;
    [InitDefaultValue]
    [Condition(nameof(Bit1), 2)]
    public float AI1;
    [InitDefaultValue]
    [Condition(nameof(Bit1), 3)]
    public float AI2;
    [InitDefaultValue]
    [Condition(nameof(Bit1), 4)]
    public float AI3;
    [InitDefaultValue]
    [Condition(nameof(Bit1), 5)]
    public float AI4;
    [InitDefaultValue]
    public short NPCType;
    [InitDefaultValue]
    [Condition(nameof(Bit2), 0)]
    public byte PlayerCount;
    [InitDefaultValue]
    [Condition(nameof(Bit2), 2)]
    public float StrengthMultiplier;
    [InitDefaultValue]
    [Condition(nameof(Bit1), 7, false)]
    public BitsByte Bit3;
    [InitDefaultValue]
    [Condition(nameof(Bit3), 0)]
    public sbyte PrettyShortHP;
    [InitDefaultValue]
    [Condition(nameof(Bit3), 1)]
    public short ShortHP;
    [InitDefaultValue]
    [Condition(nameof(Bit3), 2)]
    public int HP;
}