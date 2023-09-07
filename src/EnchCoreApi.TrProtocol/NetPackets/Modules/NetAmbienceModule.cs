using EnchCoreApi.TrProtocol.Models;
using EnchCoreApi.TrProtocol.Models.Interfaces;
using Terraria.GameContent.Ambience;

namespace EnchCoreApi.TrProtocol.NetPackets.Modules;

public partial class NetAmbienceModule : NetModulesPacket, IPlayerSlot {
    public sealed override NetModuleType ModuleType => NetModuleType.NetAmbienceModule;
    public byte PlayerSlot { get; set; }
    public int Random;
    public SkyEntityType SkyType;
}
