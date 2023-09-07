using EnchCoreApi.TrProtocol.Models.Interfaces;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class Emoji : NetPacket, IPlayerSlot {
    public sealed override MessageID Type => MessageID.Emoji;
    public byte PlayerSlot { get; set; }
    public byte Emote;
}