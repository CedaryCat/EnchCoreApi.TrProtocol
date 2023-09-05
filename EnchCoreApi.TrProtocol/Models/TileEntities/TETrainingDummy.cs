using Terraria.DataStructures;

namespace EnchCoreApi.TrProtocol.Models.TileEntities;

public partial class TETrainingDummy : TileEntity {
    public sealed override TileEntityType EntityType => TileEntityType.TETrainingDummy;
    public sealed override Point16 Position { get; set; }
    public sealed override int ID { get; set; }

    public short NPC;
}
