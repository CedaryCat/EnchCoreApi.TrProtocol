using EnchCoreApi.TrProtocol.Models;
using Terraria.GameContent.Drawing;

namespace EnchCoreApi.TrProtocol.NetPackets.Modules;

public partial class NetParticlesModule : NetModulesPacket {
    public sealed override NetModuleType ModuleType => NetModuleType.NetParticlesModule;
    public ParticleOrchestraType ParticleType;
    public ParticleOrchestraSettings Setting;
}
