using EnchCoreApi.TrProtocol.Models;

namespace EnchCoreApi.TrProtocol.NetPackets.Modules;

public partial class NetCreativeUnlocksPlayerReportModule : NetModulesPacket {
    public sealed override NetModuleType ModuleType => NetModuleType.NetCreativeUnlocksPlayerReportModule;
    public byte AlwaysZero = 0;
    public short ItemId;
    public ushort Count;
}
