using EnchCoreApi.TrProtocol.Attributes;
using EnchCoreApi.TrProtocol.Models.Interfaces;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class PlayerBuffs : NetPacket, IPlayerSlot {
    public sealed override MessageID Type => MessageID.PlayerBuffs;
    public byte PlayerSlot { get; set; }
    [ArraySize(44)]
    public ushort[] BuffTypes;
}