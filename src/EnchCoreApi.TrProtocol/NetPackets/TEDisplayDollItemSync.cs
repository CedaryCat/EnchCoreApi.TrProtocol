using EnchCoreApi.TrProtocol.Models.Interfaces;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class TEDisplayDollItemSync : NetPacket, IPlayerSlot {
    public sealed override MessageID Type => MessageID.TEDisplayDollItemSync;
    public byte PlayerSlot { get; set; }
    public int TileEntityID;
    public byte ItemSlot;
    public ushort ItemID;
    public ushort Stack;
    public byte Prefix;
}