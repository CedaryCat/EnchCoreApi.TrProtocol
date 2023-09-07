using EnchCoreApi.TrProtocol.Models.Interfaces;
using Terraria.DataStructures;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class ChestUpdates : NetPacket, IChestSlot {
    public sealed override MessageID Type => MessageID.ChestUpdates;
    public byte Operation;
    public Point16 Position;
    public short Style;
    public short ChestSlot { get; set; }
}