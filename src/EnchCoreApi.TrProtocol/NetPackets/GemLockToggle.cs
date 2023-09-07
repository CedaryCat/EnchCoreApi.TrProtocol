using Terraria.DataStructures;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class GemLockToggle : NetPacket {
    public sealed override MessageID Type => MessageID.GemLockToggle;
    public Point16 Position;
    public bool Flag;
}