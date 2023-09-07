using Microsoft.Xna.Framework;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class SyncExtraValue : NetPacket {
    public sealed override MessageID Type => MessageID.SyncExtraValue;
    public short NPCSlot;
    public int Extra;
    public Vector2 MoneyPing;
}