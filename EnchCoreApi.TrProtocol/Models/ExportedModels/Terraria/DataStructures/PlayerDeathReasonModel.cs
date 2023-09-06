using EnchCoreApi.TrProtocol.Attributes;

namespace Terraria.DataStructures {
    [TypeConvertion("Terraria.DataStructures.PlayerDeathReason")]
    public class PlayerDeathReasonModel {
        [MemberConvertion(ConvertionOption.Ignore)]
        public BitsByte Indicator;

        [MemberConvertion(ConvertionOption.Copy)]
        [Condition(nameof(Indicator), 0)]
        public int _sourcePlayerIndex;

        [MemberConvertion(ConvertionOption.Copy)]
        [Condition(nameof(Indicator), 1)]
        public int _sourceNPCIndex;

        [MemberConvertion(ConvertionOption.Copy)]
        [Condition(nameof(Indicator), 2)]
        public int _sourceProjectileLocalIndex;

        [MemberConvertion(ConvertionOption.Copy)]
        [Condition(nameof(Indicator), 3)]
        public int _sourceOtherIndex;

        [MemberConvertion(ConvertionOption.Copy)]
        [Condition(nameof(Indicator), 4)]
        public int _sourceProjectileType;

        [MemberConvertion(ConvertionOption.Copy)]
        [Condition(nameof(Indicator), 5)]
        public int _sourceItemType;

        [MemberConvertion(ConvertionOption.Copy)]
        [Condition(nameof(Indicator), 6)]
        public int _sourceItemPrefix;

        [MemberConvertion(ConvertionOption.Copy)]
        [Condition(nameof(Indicator), 7)]
        public string? _sourceCustomReason;
    }
}
