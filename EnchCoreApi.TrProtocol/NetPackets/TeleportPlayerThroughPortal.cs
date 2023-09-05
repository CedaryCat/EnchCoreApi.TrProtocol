using EnchCoreApi.TrProtocol.Models.Interfaces;
using Microsoft.Xna.Framework;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class TeleportPlayerThroughPortal : NetPacket, IOtherPlayerSlot {
    public sealed override MessageID Type => MessageID.TeleportPlayerThroughPortal;
    public byte OtherPlayerSlot { get; set; }
    public ushort Extra;
    public Vector2 Position;
    public Vector2 Velocity;
}