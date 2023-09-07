using Terraria.DataStructures;
using Terraria.GameContent.UI.WiresUI.Settings;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class MassWireOperation : NetPacket {
    public sealed override MessageID Type => MessageID.MassWireOperation;
    public Point16 Start;
    public Point16 End;
    public MultiToolMode Mode;
}