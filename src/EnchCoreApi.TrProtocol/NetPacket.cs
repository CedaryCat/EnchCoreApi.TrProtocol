using EnchCoreApi.TrProtocol.Attributes;
using EnchCoreApi.TrProtocol.Interfaces;
using System.Text;

namespace EnchCoreApi.TrProtocol {

    [AbstractModel(typeof(MessageID), nameof(Type))]
    public abstract partial class NetPacket : IAutoSerializableData {
        public abstract MessageID Type { get; }
        public unsafe abstract void ReadContent(ref void * ptr);
        public unsafe abstract void WriteContent(ref void* ptr);
        public override string ToString() {
            return $"{{{Type}}}";
        }
    }
}
