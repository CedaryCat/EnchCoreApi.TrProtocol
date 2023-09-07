using EnchCoreApi.TrProtocol.Models.Interfaces;
using Microsoft.Xna.Framework;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class MinionRestTargetUpdate : NetPacket, IPlayerSlot {
    public sealed override MessageID Type => MessageID.MinionRestTargetUpdate;
    public byte PlayerSlot { get; set; }
    public Vector2 MinionRestTargetPoint;
}