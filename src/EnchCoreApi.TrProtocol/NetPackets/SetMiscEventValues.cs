using EnchCoreApi.TrProtocol.Models.Interfaces;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class SetMiscEventValues : NetPacket, IOtherPlayerSlot {
    public sealed override MessageID Type => MessageID.SetMiscEventValues;
    public byte OtherPlayerSlot { get; set; }
    public int CreditsRollTime;
}