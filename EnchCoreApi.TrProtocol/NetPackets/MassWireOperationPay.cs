using EnchCoreApi.TrProtocol.Models.Interfaces;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class MassWireOperationPay : NetPacket, IPlayerSlot {
    public sealed override MessageID Type => MessageID.MassWireOperationPay;
    public short ItemType;
    public short Stack;
    public byte PlayerSlot { get; set; }
}