using EnchCoreApi.TrProtocol.Models.Interfaces;
using Terraria;
using Terraria.DataStructures;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class SpawnPlayer : NetPacket, IPlayerSlot {
    public sealed override MessageID Type => MessageID.SpawnPlayer;
    public byte PlayerSlot { get; set; }
    public Point16 Position;
    public int Timer;
    public short DeathsPVE;
    public short DeathsPVP;
    public PlayerSpawnContext Context;
}
