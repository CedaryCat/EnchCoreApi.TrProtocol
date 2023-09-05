using EnchCoreApi.TrProtocol.Attributes;

namespace Microsoft.Xna.Framework {

    [TypeForward]
    public interface IPackedVector<TPacked> : IPackedVector {
        TPacked PackedValue { get; set; }
    }
    [TypeForward]
    public interface IPackedVector {
        Vector4 ToVector4();

        void PackFromVector4(Vector4 vector);
    }
}
