using EnchCoreApi.TrProtocol.Attributes;

namespace Terraria.DataStructures {
    [TypeConvertion("Terraria.DataStructures.PlayerDeathReason")]
    public class PlayerDeathReasonModel {
        [MemberConvertion(ConvertionOption.Ignore)]
        public BitsByte Indicator;

        [MemberConvertion(ConvertionOption.Copy)]
        [Condition(nameof(Indicator), 0)]
        public short _sourcePlayerIndex;

        [MemberConvertion(ConvertionOption.Copy)]
        [Condition(nameof(Indicator), 1)]
        public short _sourceNPCIndex;

        [MemberConvertion(ConvertionOption.Copy)]
        [Condition(nameof(Indicator), 2)]
        public short _sourceProjectileLocalIndex;

        [MemberConvertion(ConvertionOption.Copy)]
        [Condition(nameof(Indicator), 3)]
        public byte _sourceOtherIndex;

        [MemberConvertion(ConvertionOption.Copy)]
        [Condition(nameof(Indicator), 4)]
        public short _sourceProjectileType;

        [MemberConvertion(ConvertionOption.Copy)]
        [Condition(nameof(Indicator), 5)]
        public short _sourceItemType;

        [MemberConvertion(ConvertionOption.Copy)]
        [Condition(nameof(Indicator), 6)]
        public byte _sourceItemPrefix;

        [MemberConvertion(ConvertionOption.Copy)]
        [Condition(nameof(Indicator), 7)]
        public string? _sourceCustomReason;
    }
}
