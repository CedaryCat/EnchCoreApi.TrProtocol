using EnchCoreApi.TrProtocol.Models.Interfaces;
using Microsoft.Xna.Framework;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class SyncItemCannotBeTakenByEnemies : NetPacket, IItemSlot {
    public sealed override MessageID Type => MessageID.SyncItemCannotBeTakenByEnemies;
    public short ItemSlot { get; set; }
    public Vector2 Position;
    public Vector2 Velocity;
    public short Stack;
    public byte Prefix;
    public byte Owner;
    public short ItemType;
    public bool Shimmered;
    public byte TimeLeftInWhichTheItemCannotBeTakenByEnemies;
}