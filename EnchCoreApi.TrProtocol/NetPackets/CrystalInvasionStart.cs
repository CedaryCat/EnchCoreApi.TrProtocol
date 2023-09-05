using Terraria.DataStructures;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class CrystalInvasionStart : NetPacket {
    public sealed override MessageID Type => MessageID.CrystalInvasionStart;
    public Point16 Position;
}