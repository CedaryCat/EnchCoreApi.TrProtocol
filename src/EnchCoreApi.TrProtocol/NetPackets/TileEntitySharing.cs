using EnchCoreApi.TrProtocol.Attributes;
using EnchCoreApi.TrProtocol.Models.TileEntities;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class TileEntitySharing : NetPacket {
    public sealed override MessageID Type => MessageID.TileEntitySharing;
    public int ID;
    public bool IsNew;
    [Condition(nameof(IsNew), true)]
    [ExternalMemberValue(nameof(TileEntity.NetworkSend), true)]
    public TileEntity? Entity;
}