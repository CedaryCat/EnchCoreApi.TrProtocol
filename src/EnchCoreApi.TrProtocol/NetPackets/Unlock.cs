using EnchCoreApi.TrProtocol.Models.Interfaces;
using Terraria.DataStructures;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class Unlock : NetPacket, IPlayerSlot {
    public sealed override MessageID Type => MessageID.Unlock;
    public byte PlayerSlot { get; set; }
    public Point16 Position;
}