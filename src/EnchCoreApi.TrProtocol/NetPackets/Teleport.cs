using EnchCoreApi.TrProtocol.Attributes;
using EnchCoreApi.TrProtocol.Models.Interfaces;
using Microsoft.Xna.Framework;
using Terraria;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class Teleport : NetPacket, IPlayerSlot {
    public sealed override MessageID Type => MessageID.Teleport;
    public BitsByte Bit1;
    public byte PlayerSlot { get; set; }
    public byte HighBitOfPlayerIsAlwaysZero = 0;
    public Vector2 Position;
    public byte Style;
    [Condition(nameof(Bit1), 3)]
    public int ExtraInfo;
}