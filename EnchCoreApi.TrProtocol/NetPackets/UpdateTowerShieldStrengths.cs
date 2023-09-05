using EnchCoreApi.TrProtocol.Attributes;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class UpdateTowerShieldStrengths : NetPacket {
    public sealed override MessageID Type => MessageID.UpdateTowerShieldStrengths;
    [ArraySize(4)]
    public ushort[] ShieldStrength { get; set; }
}