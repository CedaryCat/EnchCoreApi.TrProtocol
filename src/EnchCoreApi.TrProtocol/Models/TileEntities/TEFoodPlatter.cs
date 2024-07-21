using EnchCoreApi.TrProtocol.Attributes;
using Terraria.DataStructures;

namespace EnchCoreApi.TrProtocol.Models.TileEntities;

public partial class TEFoodPlatter : TileEntity {
    public sealed override TileEntityType EntityType => TileEntityType.TEFoodPlatter;
    [ExternalMember]
    [IgnoreSerialize]
    public sealed override bool NetworkSend { get; set; }
    [Condition(nameof(NetworkSend), false)]
    public sealed override int ID { get; set; }
    public sealed override Point16 Position { get; set; }

    public ItemData Item;
}
