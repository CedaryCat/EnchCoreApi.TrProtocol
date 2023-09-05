using Terraria.DataStructures;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class FoodPlatterTryPlacing : NetPacket {
    public sealed override MessageID Type => MessageID.FoodPlatterTryPlacing;
    public Point16 Position;
    public short ItemType;
    public byte Prefix;
    public short Stack;
}