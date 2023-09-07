using Terraria.DataStructures;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class RequestChestOpen : NetPacket {
    public sealed override MessageID Type => MessageID.RequestChestOpen;
    public Point16 Position;
}