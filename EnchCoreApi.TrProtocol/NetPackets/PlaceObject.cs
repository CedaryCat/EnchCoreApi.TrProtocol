using Terraria.DataStructures;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class PlaceObject : NetPacket {
    public sealed override MessageID Type => MessageID.PlaceObject;
    public Point16 Position;
    public short ObjectType;
    public short Style;
    public byte Alternate;
    public sbyte Random;
    public bool Direction;
}