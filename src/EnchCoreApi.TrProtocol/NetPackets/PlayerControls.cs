using EnchCoreApi.TrProtocol.Attributes;
using EnchCoreApi.TrProtocol.Models.Interfaces;
using Microsoft.Xna.Framework;
using Terraria;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class PlayerControls : NetPacket, IPlayerSlot {
    public sealed override MessageID Type => MessageID.PlayerControls;
    public byte PlayerSlot { get; set; }
    public BitsByte Bit1;
    public BitsByte Bit2;
    public BitsByte Bit3;
    public BitsByte Bit4;
    public byte SelectedItem;
    public Vector2 Position;
    [Condition(nameof(Bit2), 2)]
    public Vector2 Velocity;
    [Condition(nameof(Bit3), 6)]
    public Vector2 PotionOfReturnOriginalUsePosition;
    [Condition(nameof(Bit3), 6)]
    public Vector2 PotionOfReturnHomePosition;
}
