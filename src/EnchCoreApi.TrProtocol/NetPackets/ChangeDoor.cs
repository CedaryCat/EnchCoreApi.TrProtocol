using Terraria.DataStructures;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class ChangeDoor : NetPacket {
    public sealed override MessageID Type => MessageID.ChangeDoor;
    public bool ChangeType;
    public Point16 Position;
    public byte Direction;
}
