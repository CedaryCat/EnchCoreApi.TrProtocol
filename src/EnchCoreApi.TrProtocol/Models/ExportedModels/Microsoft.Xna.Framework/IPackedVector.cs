using EnchCoreApi.TrProtocol.Attributes;

namespace Microsoft.Xna.Framework {

    [TypeMigrationTarget]
    public interface IPackedVector<TPacked> : IPackedVector {
        TPacked PackedValue { get; set; }
    }
    [TypeMigrationTarget]
    public interface IPackedVector {
        Vector4 ToVector4();

        void PackFromVector4(Vector4 vector);
    }
}
