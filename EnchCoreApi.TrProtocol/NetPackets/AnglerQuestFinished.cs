namespace EnchCoreApi.TrProtocol.NetPackets {

    public partial class AnglerQuestFinished : NetPacket {
        public sealed override MessageID Type => MessageID.AnglerQuestFinished;
    }
}
