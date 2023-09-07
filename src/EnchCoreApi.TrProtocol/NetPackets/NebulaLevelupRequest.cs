using EnchCoreApi.TrProtocol.Models.Interfaces;
using Microsoft.Xna.Framework;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class NebulaLevelupRequest : NetPacket, IPlayerSlot {
    public sealed override MessageID Type => MessageID.NebulaLevelupRequest;
    public byte PlayerSlot { get; set; }
    public ushort NebulaType;
    public Vector2 Position;
}