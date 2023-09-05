using EnchCoreApi.TrProtocol.Models.Interfaces;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class AddPlayerBuff : NetPacket, IOtherPlayerSlot {
    public sealed override MessageID Type => MessageID.AddPlayerBuff;
    public byte OtherPlayerSlot { get; set; }
    public ushort BuffType;
    public int BuffTime;
}
