using Terraria.DataStructures;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class WeaponsRackTryPlacing : NetPacket {
    public sealed override MessageID Type => MessageID.WeaponsRackTryPlacing;
    public Point16 Position;
    public short ItemType;
    public byte Prefix;
    public short Stack;
}