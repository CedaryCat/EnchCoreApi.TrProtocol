using Terraria.Localization;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class StatusText : NetPacket {
    public sealed override MessageID Type => MessageID.StatusText;
    public int Max;
    public NetworkTextModel Text;
    public byte Flag;
}
