using EnchCoreApi.TrProtocol.Attributes;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class TravelMerchantItems : NetPacket {
    public sealed override MessageID Type => MessageID.TravelMerchantItems;
    [ArraySize(40)]
    public short[] ShopItems;
}