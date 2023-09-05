using EnchCoreApi.TrProtocol.Models.Interfaces;

namespace EnchCoreApi.TrProtocol.NetPackets.Mobile {

    public partial class PlayerPlatformInfo : NetPacket, IPlayerSlot {
        public sealed override MessageID Type => MessageID.PlayerPlatformInfo;
        public byte PlayerSlot { get; set; }
        public Platform PlatformId;
        public string PlayerId;
    }
}
