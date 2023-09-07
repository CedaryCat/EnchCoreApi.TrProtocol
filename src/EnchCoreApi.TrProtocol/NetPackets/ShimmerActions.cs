using EnchCoreApi.TrProtocol.Attributes;
using EnchCoreApi.TrProtocol.Models.Interfaces;
using Microsoft.Xna.Framework;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class ShimmerActions : NetPacket, INPCSlot {
    public sealed override MessageID Type => MessageID.ShimmerActions;
    public byte ShimmerType;
    [ConditionEqual(nameof(ShimmerType), 0)]
    public Vector2 ShimmerPosition;
    [ConditionEqual(nameof(ShimmerType), 1)]
    public Vector2 CoinPosition;
    [ConditionEqual(nameof(ShimmerType), 1)]
    public int CoinAmount;
    [ConditionEqual(nameof(ShimmerType), 2)]
    public short NPCSlot { get; set; }
    [ConditionEqual(nameof(ShimmerType), 2)]
    public short NPCSlotHighBits;
}