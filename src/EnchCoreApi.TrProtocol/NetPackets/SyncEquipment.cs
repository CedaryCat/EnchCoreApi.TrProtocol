using EnchCoreApi.TrProtocol.Models.Interfaces;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class SyncEquipment : NetPacket, IPlayerSlot {
    public sealed override MessageID Type => MessageID.SyncEquipment;
    public byte PlayerSlot { get; set; }
    public short ItemSlot;
    public short Stack;
    public byte Prefix;
    public short ItemType;
}
