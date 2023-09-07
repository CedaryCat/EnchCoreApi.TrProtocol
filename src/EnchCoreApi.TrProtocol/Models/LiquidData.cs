using EnchCoreApi.TrProtocol.Attributes;

namespace EnchCoreApi.TrProtocol.Models;

public partial struct LiquidData {
    public ushort TotalChanges;
    [ArraySize(nameof(TotalChanges))]
    public LiquidChange[] LiquidChanges;
}
