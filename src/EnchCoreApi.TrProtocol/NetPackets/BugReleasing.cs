using Microsoft.Xna.Framework;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class BugReleasing : NetPacket {
    public sealed override MessageID Type => MessageID.BugReleasing;
    public Point Position;
    public short NPCType;
    public byte Styl;
}