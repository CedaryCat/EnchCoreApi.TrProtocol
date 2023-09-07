using EnchCoreApi.TrProtocol.Models.Interfaces;
using Microsoft.Xna.Framework;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class InstancedItem : NetPacket, IItemSlot {
    public sealed override MessageID Type => MessageID.InstancedItem;
    public short ItemSlot { get; set; }
    public Vector2 Position;
    public Vector2 Velocity;
    public short Stack;
    public byte Prefix;
    public byte Owner;
    public short ItemType;
}
