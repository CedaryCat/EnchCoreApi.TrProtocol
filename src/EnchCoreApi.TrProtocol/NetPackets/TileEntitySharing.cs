using EnchCoreApi.TrProtocol.Attributes;
using EnchCoreApi.TrProtocol.Models.TileEntities;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class TileEntitySharing : NetPacket {
    public sealed override MessageID Type => MessageID.TileEntitySharing;
    public int ID;
    public bool IsNew;
    [Condition(nameof(IsNew), true)]
    public TileEntity? Entity;
}