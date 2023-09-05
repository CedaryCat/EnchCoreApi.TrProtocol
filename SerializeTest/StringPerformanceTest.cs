using BenchmarkDotNet.Attributes;
using EnchCoreApi.TrProtocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SerializeTest {
    public class StringPerformanceTest {
        const string text = "Terraria，启动！他真启动了吗，如启，启了吗，如启，他真启了吗，如启，他很厉害，他不是一个真的客户端。";
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
            [Benchmark]
            public unsafe void UnmanagedMemoryBinaryWrite() {
                fixed (byte* ptr = buffer) {
                    using var ms = new UnmanagedMemoryStream(ptr, bufferSize);
                    using var bw = new BinaryWriter(ms);
                    bw.Write(text);
                }
            }
            [Benchmark]
            public unsafe void UnsafeWrite() {
                fixed (void* ptr = buffer) {
                    var p = ptr;
                    CommonCode.WriteString(ref p, text);
                }
            }
            [Benchmark]
            public unsafe void UnsafeWrite2() {
                fixed (void* ptr = buffer) {
                    var p = ptr;
                    CommonCode.WriteString2(ref p, text);
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
                    CommonCode.ReadString(ref p);
                }
            }
        }
    }
}
