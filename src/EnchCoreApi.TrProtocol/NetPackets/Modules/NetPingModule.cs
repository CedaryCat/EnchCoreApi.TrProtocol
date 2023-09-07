using EnchCoreApi.TrProtocol.Models;
using Microsoft.Xna.Framework;

namespace EnchCoreApi.TrProtocol.NetPackets.Modules;

public partial class NetPingModule : NetModulesPacket {
    public sealed override NetModuleType ModuleType => NetModuleType.NetPingModule;
    public Vector2 Position;
}
