using Terraria;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class ShopOverride : NetPacket {
    public sealed override MessageID Type => MessageID.ShopOverride;
    public byte ItemSlot;
    public short ItemType;
    public short Stack;
    public byte Prefix;
    public int Value;
    public BitsByte BuyOnce; // only first bit counts
}