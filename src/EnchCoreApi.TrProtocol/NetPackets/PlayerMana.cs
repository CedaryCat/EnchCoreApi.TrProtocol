using EnchCoreApi.TrProtocol.Models.Interfaces;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class PlayerMana : NetPacket, IPlayerSlot {
    public sealed override MessageID Type => MessageID.PlayerMana;
    public byte PlayerSlot { get; set; }
    public short StatMana;
    public short StatManaMax;
}