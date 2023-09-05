using BenchmarkDotNet.Attributes;
using Microsoft.Xna.Framework;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SerializeTest {
    [MemoryDiagnoser, RankColumn]
    public unsafe class UnsafePerformanceTest : IDisposable {
        byte* buffer;
        public UnsafePerformanceTest() {
            buffer = (byte*)Marshal.AllocHGlobal(1024);
        }

        public void Dispose() {
            Marshal.FreeHGlobal((IntPtr)buffer);
        }
        [Benchmark]
        public void Test1() {
            var b = new Span<byte>(buffer, 50);

            var e = new Example();

            fixed (byte* ptr_origi = b) {
                var ptr = ptr_origi;

                e.Pos = *((Vector2*)ptr);
                ptr += sizeof(Vector2);

                e.Velo = *((Vector2*)ptr);
                ptr += sizeof(Vector2);

                e.Size = *((Vector2*)ptr);
                ptr += sizeof(Vector2);

                e.Length = *((int*)ptr);
                ptr += sizeof(int);
            }

        }
        [Benchmark]
        public void Test2() {
            var b = new Span<byte>(buffer, 50);

            var e = new Example();

            fixed (void* ptr_origi = b) {
                var ptr = ptr_origi;

                e.Pos = Unsafe.Read<Vector2>(ptr);
                ptr = Unsafe.Add<Vector2>(ptr, 1);

                e.Velo = Unsafe.Read<Vector2>(ptr);
                ptr = Unsafe.Add<Vector2>(ptr, 1);

                e.Size = Unsafe.Read<Vector2>(ptr);
                ptr = Unsafe.Add<Vector2>(ptr, 1);

                e.Length = Unsafe.Read<int>(ptr);
                ptr = Unsafe.Add<int>(ptr, 1);
            }
        }
        [Benchmark]
        public void Test3() {
            var b = new Span<byte>(buffer, 50);

            var e = new Example();

            fixed (byte* ptr_origi = b) {
                var ptr = ptr_origi;

                e.Pos = Unsafe.Read<Vector2>(ptr);
                ptr += sizeof(Vector2);

                e.Velo = Unsafe.Read<Vector2>(ptr);
                ptr += sizeof(Vector2);

                e.Size = Unsafe.Read<Vector2>(ptr);
                ptr += sizeof(Vector2);

                e.Length = Unsafe.Read<int>(ptr);
                ptr += sizeof(int);
            }
        }

        [Benchmark]
        public void Test4() {
            var b = new Span<byte>(buffer, 50);

            var e = new Example();

            fixed (void* ptr_origi = b) {
                var ptr = ptr_origi;

                e.Pos = Unsafe.Read<Vector2>(ptr);
                ptr = Unsafe.Add<byte>(ptr, sizeof(Vector2));

                e.Velo = Unsafe.Read<Vector2>(ptr);
                ptr = Unsafe.Add<byte>(ptr, sizeof(Vector2));

                e.Size = Unsafe.Read<Vector2>(ptr);
                ptr = Unsafe.Add<byte>(ptr, sizeof(Vector2));

                e.Length = Unsafe.Read<int>(ptr);
                ptr = Unsafe.Add<byte>(ptr, sizeof(int));
            }
        }

        [Benchmark]
        public void Test5() {
            var e = new Example();

            using var br = new BinaryReader(new UnmanagedMemoryStream(buffer, 50));

            e.Pos = new Vector2(br.ReadSingle(), br.ReadSingle());
            e.Velo = new Vector2(br.ReadSingle(), br.ReadSingle());
            e.Size = new Vector2(br.ReadSingle(), br.ReadSingle());
            e.Length = br.ReadInt32();
        }

        [Benchmark]
        public void Test6() {
            var b = new byte[50];
            var e = new Example();

            using var br = new BinaryReader(new MemoryStream(b));

            e.Pos = new Vector2(br.ReadSingle(), br.ReadSingle());
            e.Velo = new Vector2(br.ReadSingle(), br.ReadSingle());
            e.Size = new Vector2(br.ReadSingle(), br.ReadSingle());
            e.Length = br.ReadInt32();
        }
    }
    public class Example {
        public Vector2 Pos, Velo, Size;
        public int Length;
    }
}
