using EnchCoreApi.TrProtocol.Attributes;
using EnchCoreApi.TrProtocol.Interfaces;
using EnchCoreApi.TrProtocol.Models;

namespace EnchCoreApi.TrProtocol.NetPackets {

    [AbstractModel(typeof(NetModuleType), nameof(ModuleType))]
    public abstract partial class NetModulesPacket : NetPacket, IAutoSerializableData {
        public sealed override MessageID Type => MessageID.NetModules;
        public abstract NetModuleType ModuleType { get; }

        public override string ToString() {
            return $"{{{Type},{ModuleType}}}";
        }
    }
}
