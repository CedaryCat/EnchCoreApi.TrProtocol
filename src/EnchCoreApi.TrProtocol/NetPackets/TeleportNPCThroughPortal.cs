using EnchCoreApi.TrProtocol.Models.Interfaces;
using Microsoft.Xna.Framework;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class TeleportNPCThroughPortal : NetPacket, INPCSlot {
    public sealed override MessageID Type => MessageID.TeleportNPCThroughPortal;
    public short NPCSlot { get; set; }
    public ushort Extra;
    public Vector2 Position;
    public Vector2 Velocity;
}