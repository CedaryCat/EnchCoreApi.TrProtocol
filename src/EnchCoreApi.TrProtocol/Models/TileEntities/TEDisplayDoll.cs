using EnchCoreApi.TrProtocol.Attributes;
using Terraria;
using Terraria.DataStructures;

namespace EnchCoreApi.TrProtocol.Models.TileEntities;

public partial class TEDisplayDoll : TileEntity {
    public sealed override TileEntityType EntityType => TileEntityType.TEDisplayDoll;
    [ExternalMember]
    [IgnoreSerialize]
    public sealed override bool NetworkSend { get; set; }
    [Condition(nameof(NetworkSend), false)]
    public sealed override int ID { get; set; }
    public sealed override Point16 Position { get; set; }

    public BitsByte ItemsSerializableIndicator;

    public BitsByte DyesSerializableIndicator;

    [ConditionArray(nameof(ItemsSerializableIndicator), 0)]
    [ArraySize(8)]
    [InitNullable]
    public ItemData[] Items = new ItemData[8];

    [ConditionArray(nameof(DyesSerializableIndicator), 0)]
    [ArraySize(8)]
    [InitNullable]
    public ItemData[] Dyes = new ItemData[8];
}
