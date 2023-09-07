using EnchCoreApi.TrProtocol.Models.Interfaces;
using Terraria.DataStructures;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class ReadSign : NetPacket, IPlayerSlot {
    public sealed override MessageID Type => MessageID.ReadSign;
    public short SignSlot;
    public Point16 Position;
    public string Text;
    public byte PlayerSlot { get; set; }
    public byte Bit1;
}