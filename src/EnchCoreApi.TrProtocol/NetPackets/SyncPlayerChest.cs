using EnchCoreApi.TrProtocol.Attributes;
using Terraria.DataStructures;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class SyncPlayerChest : NetPacket {
    public sealed override MessageID Type => MessageID.SyncPlayerChest;
    public short Chest;
    public Point16 Position;
    public byte NameLength;
    [ConditionGreaterThan(nameof(NameLength), 0), ConditionLessThanEqual(nameof(NameLength), 20)]
    public string? Name;
}