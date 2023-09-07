using EnchCoreApi.TrProtocol.Models;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class FishOutNPC : NetPacket {
    public sealed override MessageID Type => MessageID.FishOutNPC;
    public PointU16 Position;
    public short Start;
}