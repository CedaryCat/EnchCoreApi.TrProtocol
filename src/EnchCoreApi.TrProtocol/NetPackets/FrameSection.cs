using Terraria.DataStructures;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class FrameSection : NetPacket {
    public sealed override MessageID Type => MessageID.FrameSection;
    public Point16 Start { get; set; }
    public Point16 End { get; set; }
}
