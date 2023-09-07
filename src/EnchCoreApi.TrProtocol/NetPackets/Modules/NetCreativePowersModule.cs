using EnchCoreApi.TrProtocol.Interfaces;
using EnchCoreApi.TrProtocol.Models;

namespace EnchCoreApi.TrProtocol.NetPackets.Modules;

public sealed partial class NetCreativePowersModule : NetModulesPacket, IExtraData {
    public sealed override NetModuleType ModuleType => NetModuleType.NetCreativePowersModule;
    public CreativePowerTypes PowerType;
}
