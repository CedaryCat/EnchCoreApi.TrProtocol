using EnchCoreApi.TrProtocol.Models;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.NetModules;

namespace EnchCoreApi.TrProtocol.NetPackets.Modules;

public partial class NetTeleportPylonModule : NetModulesPacket {
    public sealed override NetModuleType ModuleType => NetModuleType.NetTeleportPylonModule;
    public NetTeleportPylonModule_SubPacketType PylonPacketType { get; set; }
    public Point16 Position { get; set; }
    public TeleportPylonType PylonType { get; set; }
}
