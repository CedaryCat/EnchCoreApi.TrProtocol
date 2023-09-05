using EnchCoreApi.TrProtocol.Attributes;

namespace Terraria;
[TypeMigrationTarget]
public enum PlayerSpawnContext {
    ReviveFromDeath,
    SpawningIntoWorld,
    RecallFromItem
}
