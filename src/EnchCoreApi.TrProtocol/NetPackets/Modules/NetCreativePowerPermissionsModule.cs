using EnchCoreApi.TrProtocol.Models;

namespace EnchCoreApi.TrProtocol.NetPackets.Modules;
public partial class NetCreativePowerPermissionsModule : NetModulesPacket {
    public sealed override NetModuleType ModuleType => NetModuleType.NetCreativePowerPermissionsModule;
    public byte AlwaysZero = 0;
    public ushort PowerId;
    public byte Level;
}