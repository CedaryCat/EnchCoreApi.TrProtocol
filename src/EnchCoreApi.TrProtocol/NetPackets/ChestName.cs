using EnchCoreApi.TrProtocol.Models.Interfaces;
using Terraria.DataStructures;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class ChestName : NetPacket, IChestSlot {
    public sealed override MessageID Type => MessageID.ChestName;
    public short ChestSlot { get; set; }
    public Point16 Position;
    public string Name;
}