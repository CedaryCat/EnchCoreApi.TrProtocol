using EnchCoreApi.TrProtocol.Models.Interfaces;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class LoadPlayer : NetPacket, IPlayerSlot {
    public sealed override MessageID Type => MessageID.LoadPlayer;
    public byte PlayerSlot { get; set; }
    public bool ServerWantsToRunCheckBytesInClientLoopThread;
}
