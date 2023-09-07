using EnchCoreApi.TrProtocol.Models.Interfaces;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class UpdatePlayerLuckFactors : NetPacket, IPlayerSlot {
    public sealed override MessageID Type => MessageID.UpdatePlayerLuckFactors;
    public byte PlayerSlot { get; set; }
    public int LadyBugTime;
    public float Torch;
    public byte Potion;
    public bool HasGardenGnomeNearby;
    public float Equip;
    public float Coin;
}