using Terraria.DataStructures;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class ItemFrameTryPlacing : NetPacket {
    public sealed override MessageID Type => MessageID.ItemFrameTryPlacing;
    public Point16 Position;
    public short ItemType;
    public byte Prefix;
    public short Stack;
}