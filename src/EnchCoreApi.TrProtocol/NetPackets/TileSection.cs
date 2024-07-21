using EnchCoreApi.TrProtocol.Interfaces;
using EnchCoreApi.TrProtocol.Models;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class TileSection : NetPacket, ILengthDependent {
    public sealed override MessageID Type => MessageID.TileSection;
    public SectionData Data;
}
