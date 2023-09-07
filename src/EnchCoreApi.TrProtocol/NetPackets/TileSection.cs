using EnchCoreApi.TrProtocol.Models;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class TileSection : NetPacket {
    public sealed override MessageID Type => MessageID.TileSection;
    public SectionData Data;
}
