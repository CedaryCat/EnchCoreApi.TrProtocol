using EnchCoreApi.TrProtocol.Models;

namespace EnchCoreApi.TrProtocol.NetPackets.Modules;
public partial class NetLiquidModule : NetModulesPacket {
    public sealed override NetModuleType ModuleType => NetModuleType.NetLiquidModule;
    public LiquidData LiquidChanges;
}
