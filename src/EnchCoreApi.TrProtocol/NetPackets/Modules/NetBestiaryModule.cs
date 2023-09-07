using EnchCoreApi.TrProtocol.Attributes;
using EnchCoreApi.TrProtocol.Models;
using Terraria.GameContent.NetModules;

namespace EnchCoreApi.TrProtocol.NetPackets.Modules;
public partial class NetBestiaryModule : NetModulesPacket {
    public sealed override NetModuleType ModuleType => NetModuleType.NetBestiaryModule;
    public NetBestiaryModule_BestiaryUnlockType UnlockType;
    public short NPCType;
    [ConditionEqual(nameof(UnlockType), 0)]
    public ushort KillCount;
}
