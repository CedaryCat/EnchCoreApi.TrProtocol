using Terraria.DataStructures;

namespace EnchCoreApi.TrProtocol.Models.TileEntities;

public partial class TEFoodPlatter : TileEntity {
    public sealed override TileEntityType EntityType => TileEntityType.TEFoodPlatter;
    public sealed override Point16 Position { get; set; }
    public sealed override int ID { get; set; }

    public ItemData Item;
}
