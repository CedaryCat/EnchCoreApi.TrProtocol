using EnchCoreApi.TrProtocol.Models.Interfaces;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class AnglerQuestCountSync : NetPacket, IPlayerSlot {
    public sealed override MessageID Type => MessageID.AnglerQuestCountSync;
    public byte PlayerSlot { get; set; }
    public int AnglerQuestsFinished;
    public int GolferScoreAccumulated;
}
