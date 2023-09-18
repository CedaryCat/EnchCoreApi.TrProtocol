using EnchCoreApi.TrProtocol.Attributes;

namespace Terraria;
[TypeMigrationTarget]
public enum PlayerSpawnContext : byte {
    ReviveFromDeath,
    SpawningIntoWorld,
    RecallFromItem
}
