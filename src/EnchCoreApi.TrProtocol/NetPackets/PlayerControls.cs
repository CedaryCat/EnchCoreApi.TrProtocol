using EnchCoreApi.TrProtocol.Attributes;
using EnchCoreApi.TrProtocol.Models;
using EnchCoreApi.TrProtocol.Models.Interfaces;
using Microsoft.Xna.Framework;
using Terraria;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class PlayerControls : NetPacket, IPlayerSlot {
    public sealed override MessageID Type => MessageID.PlayerControls;
    public byte PlayerSlot { get; set; }
    public PlayerControlData PlayerControlData;
    public PlayerMiscData1 PlayerMiscData1;
    private bool HasValue => PlayerMiscData1.HasVelocity;
    public PlayerMiscData2 PlayerMiscData2;
    private bool CanReturnWithPotionOfReturn => PlayerMiscData2.CanReturnWithPotionOfReturn;
    public PlayerMiscData3 PlayerMiscData3;
    public byte SelectedItem;
    public Vector2 Position;
    [Condition(nameof(HasValue), true)]
    public Vector2 Velocity;
    [Condition(nameof(CanReturnWithPotionOfReturn), true)]
    public Vector2 PotionOfReturnOriginalUsePosition;
    [Condition(nameof(CanReturnWithPotionOfReturn), true)]
    public Vector2 PotionOfReturnHomePosition;
}
