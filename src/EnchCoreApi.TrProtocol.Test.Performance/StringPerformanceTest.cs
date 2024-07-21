using BenchmarkDotNet.Attributes;
using EnchCoreApi.TrProtocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnchCoreApi.TrProtocol.Test.Performance
{
    public class StringPerformanceTest {
        const string text = "原神，启动！原神，启动！原神，启动！原神，启动！原神，启动！原神，启动！原神，启动！";
        //const int testSize = 100000000;
        const int bufferSize = 1024;
        public readonly byte[] buffer = new byte[bufferSize];
        public MemoryStream memory;
        public BinaryReader br;
        public BinaryWriter bw;
        public StringPerformanceTest() {
            memory = new MemoryStream(buffer);
            br = new BinaryReader(memory);
            bw = new BinaryWriter(memory);
            bw.Write(text);
            memory.Position = 0;
        }

        [MemoryDiagnoser, RankColumn]
        public class StringTestWrite : StringPerformanceTest {
            [Benchmark]
            public void MemoryBinaryWrite() {
                bw.Write(text);
                memory.Position = 0;
            }
            [Benchmark]
            public void MemoryBinaryWrite_Normal() {
                using var ms = new MemoryStream(buffer);
                using var bw = new BinaryWriter(ms);
                bw.Write(text);
            }
            public readonly byte[] buffer1 = new byte[bufferSize];
            [Benchmark]
            public unsafe void UnmanagedMemoryBinaryWrite() {
                fixed (byte* ptr = buffer1) {
                    using var ms = new UnmanagedMemoryStream(ptr, bufferSize);
                    using var bw = new BinaryWriter(ms);
                    bw.Write(text);
                }
            }
            public readonly byte[] buffer2 = new byte[bufferSize];
            [Benchmark]
            public unsafe void UnsafeWrite() {
                fixed (void* ptr = buffer2) {
                    var p = ptr;
                    CommonCode.WriteString(ref p, text);
                }
            }
            public readonly byte[] buffer3 = new byte[bufferSize];
            [Benchmark]
            public unsafe void UnsafeWrite2()
            {
                fixed (void* ptr = buffer3)
                {
                    var p = ptr;
                    CommonCode.WriteString2(ref p, text);
                }
            }
            public readonly byte[] buffer4 = new byte[bufferSize];
            [Benchmark]
            public unsafe void UnsafeWrite3()
            {
                fixed (void* ptr = buffer4)
                {
                    var p = ptr;
                    CommonCode.WriteString3(ref p, text);
                }
            }
        }
        [MemoryDiagnoser, RankColumn]
        public class StringTestRead : StringPerformanceTest {
            [Benchmark]
            public void MemoryBinaryRead() {
                br.ReadString();
                memory.Position = 0;
            }
            [Benchmark]
            public void MemoryBinaryRead_Normal() {
                using var ms = new MemoryStream(buffer);
                using var br = new BinaryReader(ms);
                br.ReadString();
            }
            [Benchmark]
            public unsafe void UnmanagedMemoryBinaryRead() {
                fixed (byte* ptr = buffer) {
                    using var ms = new UnmanagedMemoryStream(ptr, bufferSize);
                    using var br = new BinaryReader(ms);
                    br.ReadString();
                }
            }
            [Benchmark]
            public unsafe void UnsafeRead() {
                fixed (void* ptr = buffer) {
                    var p = ptr;
                    CommonCode.ReadString(ref p);
                }
            }
            [Benchmark]
            public unsafe void UnsafeRead2() {
                fixed (void* ptr = buffer) {
                    var p = ptr;
                    CommonCode.ReadString2(ref p);
                }
            }
            [Benchmark]
            public unsafe void UnsafeRead3()
            {
                fixed (void* ptr = buffer)
                {
                    var p = ptr;
                    CommonCode.ReadString3(ref p);
                }
            }
            [Benchmark]
            public unsafe void UnsafeRead_Ex()
            {
                fixed (void* ptr = buffer)
                {
                    var p = ptr;
                    CommonCode.ReadString_Ex(ref p);
                }
            }
        }
    }
}
