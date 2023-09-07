using EnchCoreApi.TrProtocol.Models;

namespace EnchCoreApi.TrProtocol.NetPackets.Modules;
public partial class NetCreativeUnlocksModule : NetModulesPacket {
    public sealed override NetModuleType ModuleType => NetModuleType.NetCreativeUnlocksModule;
    public short ItemId;
    public ushort Count;
}
