using EnchCoreApi.TrProtocol.Models.Interfaces;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class MurderSomeoneElsesProjectile : NetPacket, IOtherPlayerSlot {
    public sealed override MessageID Type => MessageID.MurderSomeoneElsesProjectile;
    public byte OtherPlayerSlot { get; set; }
    public byte HighBitOfPlayerIsAlwaysZero = 0;
    public byte AI1;
}