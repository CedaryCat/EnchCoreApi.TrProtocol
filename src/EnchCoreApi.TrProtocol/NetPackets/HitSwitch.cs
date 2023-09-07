using Terraria.DataStructures;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class HitSwitch : NetPacket {
    public sealed override MessageID Type => MessageID.HitSwitch;
    public Point16 Position;
}