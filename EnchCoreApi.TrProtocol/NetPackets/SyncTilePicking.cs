using EnchCoreApi.TrProtocol.Models.Interfaces;
using Terraria.DataStructures;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class SyncTilePicking : NetPacket, IPlayerSlot {
    public sealed override MessageID Type => MessageID.SyncTilePicking;
    public byte PlayerSlot { get; set; }
    public Point16 Position;
    public byte Damage;
}