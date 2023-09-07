namespace EnchCoreApi.TrProtocol.NetPackets {

    public partial class AnglerQuest : NetPacket {
        public sealed override MessageID Type => MessageID.AnglerQuest;
        public byte QuestType;
        public bool Finished;
    }
}
