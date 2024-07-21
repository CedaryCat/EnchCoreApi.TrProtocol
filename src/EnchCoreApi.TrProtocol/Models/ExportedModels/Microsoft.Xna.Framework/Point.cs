using EnchCoreApi.TrProtocol.Attributes;
using EnchCoreApi.TrProtocol.Interfaces;

namespace Microsoft.Xna.Framework;

[TypeMigrationTarget]
public struct Point : ISoildSerializableData {
    public static Point[] Array = System.Array.Empty<Point>();

    public int X;

    public int Y;

    public static readonly Point _zero = default(Point);

    public static Point Zero => _zero;

    public Point(int x, int y) {
        X = x;
        Y = y;
    }

    public static bool operator ==(Point a, Point b) {
        return a.Equals(b);
    }

    public static bool operator !=(Point a, Point b) {
        return a.X != b.X || a.Y != b.Y;
    }

    public override bool Equals(object? obj) {
        if (obj is Point point)
            return point.X == X && point.Y == Y;
        return false;
    }

    public override int GetHashCode() {
        return X.GetHashCode() + Y.GetHashCode();
    }
}
