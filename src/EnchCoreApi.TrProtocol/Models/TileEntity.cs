using EnchCoreApi.TrProtocol.Attributes;
using EnchCoreApi.TrProtocol.Interfaces;
using Terraria.DataStructures;

namespace EnchCoreApi.TrProtocol.Models.TileEntities {

    [AbstractModel(typeof(TileEntityType), nameof(EntityType))]
    public abstract partial class TileEntity : IAutoSerializableData {
        public abstract TileEntityType EntityType { get; }
        [Condition(nameof(NetworkSend), false)]
        public abstract int ID { get; set; }
        public abstract Point16 Position { get; set; }
        [ExternalMember]
        [IgnoreSerialize]
        public abstract bool NetworkSend { get; set; }

        public unsafe abstract void ReadContent(ref void* ptr);

        public unsafe abstract void WriteContent(ref void* ptr);
    }
}
