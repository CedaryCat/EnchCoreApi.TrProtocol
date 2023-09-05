using Terraria.DataStructures;

namespace EnchCoreApi.TrProtocol.Models.TileEntities;

public partial class TELogicSensor : TileEntity {
    public sealed override TileEntityType EntityType => TileEntityType.TELogicSensor;
    public sealed override Point16 Position { get; set; }
    public sealed override int ID { get; set; }

    public LogicCheckType LogicCheck;

    public bool On;
}