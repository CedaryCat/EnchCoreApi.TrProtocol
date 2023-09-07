using EnchCoreApi.TrProtocol.Attributes;
using EnchCoreApi.TrProtocol.Interfaces;
using Microsoft.Xna.Framework;
using Terraria;

namespace EnchCoreApi.TrProtocol.NetPackets;

public sealed partial class SyncNPC : NetPacket, IExtraData {
    public sealed override MessageID Type => MessageID.SyncNPC;
    public short NPCSlot;
    public Vector2 Offset;
    public Vector2 Velocity;
    public ushort Target;
    public BitsByte Bit1;
    public BitsByte Bit2;
    [Condition(nameof(Bit1), 2)]
    public float AI1;
    [Condition(nameof(Bit1), 3)]
    public float AI2;
    [Condition(nameof(Bit1), 4)]
    public float AI3;
    [Condition(nameof(Bit1), 5)]
    public float AI4;
    public short NPCType;
    [Condition(nameof(Bit2), 0)]
    public byte PlayerCount;
    [Condition(nameof(Bit2), 2)]
    public float StrengthMultiplier;
    [Condition(nameof(Bit1), 7, false)]
    public BitsByte Bit3;
    [Condition(nameof(Bit3), 0)]
    public sbyte PrettyShortHP;
    [Condition(nameof(Bit3), 1)]
    public short ShortHP;
    [Condition(nameof(Bit3), 2)]
    public int HP;
}