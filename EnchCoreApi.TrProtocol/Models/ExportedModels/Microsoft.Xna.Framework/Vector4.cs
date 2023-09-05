﻿using EnchCoreApi.TrProtocol.Attributes;
using EnchCoreApi.TrProtocol.Interfaces;

namespace Microsoft.Xna.Framework;

[TypeForward]
public struct Vector4 : ISoildSerializableData, ISequentialSerializableData<Vector4> {
    public Vector4 SequentialData {
        get => this;
        set => this = value;
    }
    public static Vector4[] Array = System.Array.Empty<Vector4>();

    public float W;

    public float X;

    public float Y;

    public float Z;

    public static Vector4 _zero = default(Vector4);

    public static readonly Vector4 _one = new Vector4(1f, 1f, 1f, 1f);

    public static Vector4 One => _one;

    public static Vector4 Zero => _zero;

    public Vector4(float x, float y, float z, float w) {
        X = x;
        Y = y;
        Z = z;
        W = w;
    }

    public Vector4(Vector2 value, float z, float w) {
        X = value.X;
        Y = value.Y;
        Z = z;
        W = w;
    }

    public Vector4(Vector3 value, float w) {
        X = value.X;
        Y = value.Y;
        Z = value.Z;
        W = w;
    }

    public Vector4(float value) {
        X = (Y = (Z = (W = value)));
    }

    public static Vector4 Lerp(Vector4 value1, Vector4 value2, float amount) {
        Vector4 result = default(Vector4);
        result.X = value1.X + (value2.X - value1.X) * amount;
        result.Y = value1.Y + (value2.Y - value1.Y) * amount;
        result.Z = value1.Z + (value2.Z - value1.Z) * amount;
        result.W = value1.W + (value2.W - value1.W) * amount;
        return result;
    }

    public static void Lerp(ref Vector4 value1, ref Vector4 value2, float amount, out Vector4 result) {
        result.X = value1.X + (value2.X - value1.X) * amount;
        result.Y = value1.Y + (value2.Y - value1.Y) * amount;
        result.Z = value1.Z + (value2.Z - value1.Z) * amount;
        result.W = value1.W + (value2.W - value1.W) * amount;
    }

    public static Vector4 operator -(Vector4 value) {
        Vector4 result = default(Vector4);
        result.X = 0f - value.X;
        result.Y = 0f - value.Y;
        result.Z = 0f - value.Z;
        result.W = 0f - value.W;
        return result;
    }

    public static Vector4 operator *(Vector4 value1, Vector4 value2) {
        Vector4 result = default(Vector4);
        result.X = value1.X * value2.X;
        result.Y = value1.Y * value2.Y;
        result.Z = value1.Z * value2.Z;
        result.W = value1.W * value2.W;
        return result;
    }

    public static Vector4 operator *(Vector4 value1, float scaleFactor) {
        Vector4 result = default(Vector4);
        result.X = value1.X * scaleFactor;
        result.Y = value1.Y * scaleFactor;
        result.Z = value1.Z * scaleFactor;
        result.W = value1.W * scaleFactor;
        return result;
    }

    public static Vector4 operator *(float scaleFactor, Vector4 value1) {
        Vector4 result = default(Vector4);
        result.X = value1.X * scaleFactor;
        result.Y = value1.Y * scaleFactor;
        result.Z = value1.Z * scaleFactor;
        result.W = value1.W * scaleFactor;
        return result;
    }

    public static Vector4 operator /(Vector4 value1, Vector4 value2) {
        Vector4 result = default(Vector4);
        result.X = value1.X / value2.X;
        result.Y = value1.Y / value2.Y;
        result.Z = value1.Z / value2.Z;
        result.W = value1.W / value2.W;
        return result;
    }

    public static Vector4 operator /(Vector4 value1, float divider) {
        float num = 1f / divider;
        Vector4 result = default(Vector4);
        result.X = value1.X * num;
        result.Y = value1.Y * num;
        result.Z = value1.Z * num;
        result.W = value1.W * num;
        return result;
    }

    public bool Equals(Vector4 other) {
        return X == other.X && Y == other.Y && Z == other.Z && W == other.W;
    }

    public override bool Equals(object? obj) {
        if (obj is Vector4 vector)
            return this == vector;
        return false;
    }

    public override int GetHashCode() {
        return X.GetHashCode() + Y.GetHashCode() + Z.GetHashCode() + W.GetHashCode();
    }

    public static bool operator ==(Vector4 p1, Vector4 p2) {
        return p1.X == p2.X && p1.Y == p2.Y && p1.Z == p2.Z && p1.W == p2.W;
    }

    public static bool operator !=(Vector4 p1, Vector4 p2) {
        return p1.X != p2.X || p1.Y != p2.Y || p1.Z != p2.Z || p1.W != p2.W;
    }

    public static Vector4 operator +(Vector4 p1, Vector4 p2) {
        Vector4 result = default(Vector4);
        result.X = p1.X + p2.X;
        result.Y = p1.Y + p2.Y;
        result.Z = p1.Z + p2.Z;
        result.W = p1.W + p2.W;
        return result;
    }

    public static Vector4 operator -(Vector4 p1, Vector4 p2) {
        Vector4 result = default(Vector4);
        result.X = p1.X - p2.X;
        result.Y = p1.Y - p2.Y;
        result.Z = p1.Z - p2.Z;
        result.W = p1.W - p2.W;
        return result;
    }

    public static Vector4 Max(Vector4 value1, Vector4 value2) {
        Vector4 result = default(Vector4);
        result.X = ((value1.X > value2.X) ? value1.X : value2.X);
        result.Y = ((value1.Y > value2.Y) ? value1.Y : value2.Y);
        result.Z = ((value1.Z > value2.Z) ? value1.Z : value2.Z);
        result.W = ((value1.W > value2.W) ? value1.W : value2.W);
        return result;
    }

    public static void Max(ref Vector4 value1, ref Vector4 value2, out Vector4 result) {
        result.X = ((value1.X > value2.X) ? value1.X : value2.X);
        result.Y = ((value1.Y > value2.Y) ? value1.Y : value2.Y);
        result.Z = ((value1.Z > value2.Z) ? value1.Z : value2.Z);
        result.W = ((value1.W > value2.W) ? value1.W : value2.W);
    }
}
