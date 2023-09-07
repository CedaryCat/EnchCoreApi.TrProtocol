using EnchCoreApi.TrProtocol.Models.Interfaces;
using Terraria.DataStructures;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class NPCHome : NetPacket, INPCSlot {
    public sealed override MessageID Type => MessageID.NPCHome;
    public short NPCSlot { get; set; }
    public Point16 Position;
    public byte Homeless;
}