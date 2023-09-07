using Microsoft.Xna.Framework;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class SyncRevengeMarker : NetPacket {
    public sealed override MessageID Type => MessageID.SyncRevengeMarker;
    public int ID;
    public Vector2 Position;
    public int NetID;
    public float Percent;
    public int NPCType;
    public int NPCAI;
    public int CoinValue;
    public float BaseValue;
    public bool SpawnFromStatue;
}