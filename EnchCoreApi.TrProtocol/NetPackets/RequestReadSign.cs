using Terraria.DataStructures;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class RequestReadSign : NetPacket {
    public sealed override MessageID Type => MessageID.RequestReadSign;
    public Point16 Position;
}