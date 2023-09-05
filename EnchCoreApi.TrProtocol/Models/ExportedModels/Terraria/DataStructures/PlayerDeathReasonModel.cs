using EnchCoreApi.TrProtocol.Attributes;

namespace Terraria.DataStructures {
    [TypeConvertion("Terraria.DataStructures.PlayerDeathReason")]
    public class PlayerDeathReasonModel {
        [ConvertionOption(ConvertionOption.Ignore)]
        public BitsByte Indicator;

        [ConvertionOption(ConvertionOption.Copy)]
        [Condition(nameof(Indicator), 0)]
        public int _sourcePlayerIndex;

        [ConvertionOption(ConvertionOption.Copy)]
        [Condition(nameof(Indicator), 1)]
        public int _sourceNPCIndex;

        [ConvertionOption(ConvertionOption.Copy)]
        [Condition(nameof(Indicator), 2)]
        public int _sourceProjectileLocalIndex;

        [ConvertionOption(ConvertionOption.Copy)]
        [Condition(nameof(Indicator), 3)]
        public int _sourceOtherIndex;

        [ConvertionOption(ConvertionOption.Copy)]
        [Condition(nameof(Indicator), 4)]
        public int _sourceProjectileType;

        [ConvertionOption(ConvertionOption.Copy)]
        [Condition(nameof(Indicator), 5)]
        public int _sourceItemType;

        [ConvertionOption(ConvertionOption.Copy)]
        [Condition(nameof(Indicator), 6)]
        public int _sourceItemPrefix;

        [ConvertionOption(ConvertionOption.Copy)]
        [Condition(nameof(Indicator), 7)]
        public string? _sourceCustomReason;
    }
}
