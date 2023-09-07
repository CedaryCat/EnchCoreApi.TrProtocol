namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class ClientSyncedInventory : NetPacket {
    public sealed override MessageID Type => MessageID.ClientSyncedInventory;
}