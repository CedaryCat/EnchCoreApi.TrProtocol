using EnchCoreApi.TrProtocol.Attributes;

namespace Terraria;
[TypeForward]
public enum PlayerSpawnContext {
    ReviveFromDeath,
    SpawningIntoWorld,
    RecallFromItem
}
