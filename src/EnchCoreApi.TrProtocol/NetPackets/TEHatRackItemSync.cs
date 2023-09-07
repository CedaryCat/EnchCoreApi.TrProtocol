using EnchCoreApi.TrProtocol.Models.Interfaces;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class TEHatRackItemSync : NetPacket, IPlayerSlot {
    public sealed override MessageID Type => MessageID.TEHatRackItemSync;
    public byte PlayerSlot { get; set; }
    public int TileEntityID;
    public byte ItemSlot;
    public ushort ItemID;
    public ushort Stack;
    public byte Prefix;
}