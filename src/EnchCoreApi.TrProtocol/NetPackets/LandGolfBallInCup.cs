using EnchCoreApi.TrProtocol.Models;
using EnchCoreApi.TrProtocol.Models.Interfaces;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class LandGolfBallInCup : NetPacket, IOtherPlayerSlot {
    public sealed override MessageID Type => MessageID.LandGolfBallInCup;
    public byte OtherPlayerSlot { get; set; }
    public PointU16 Position;
    public ushort Hits;
    public ushort ProjType;
}