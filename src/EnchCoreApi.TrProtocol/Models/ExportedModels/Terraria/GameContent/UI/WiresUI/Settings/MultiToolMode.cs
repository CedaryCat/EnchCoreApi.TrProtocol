using EnchCoreApi.TrProtocol.Attributes;

namespace Terraria.GameContent.UI.WiresUI.Settings;

[TypeMigrationTarget]
[Flags]
public enum MultiToolMode {
    Red = 1,
    Green = 2,
    Blue = 4,
    Yellow = 8,
    Actuator = 16,
    Cutter = 32
}
