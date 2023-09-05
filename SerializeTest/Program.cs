using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using EnchCoreApi.TrProtocol;
using EnchCoreApi.TrProtocol.NetPackets;
using System.CodeDom;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Terraria.DataStructures;

namespace SerializeTest {
    internal class Program {
        static unsafe void Main(string[] args) {
            BenchmarkRunner.Run<StringPerformanceTest.StringTestWrite>(ManualConfig.Create(DefaultConfig.Instance).WithOptions(ConfigOptions.DisableOptimizationsValidator));
            BenchmarkRunner.Run<StringPerformanceTest.StringTestRead>(ManualConfig.Create(DefaultConfig.Instance).WithOptions(ConfigOptions.DisableOptimizationsValidator));
            Console.ReadLine();
            //_ = new CorrectnessTest();


            //var deadResonModel = new PlayerDeathReasonModel();
            //PlayerDeathReason b = deadResonModel;
            //deadResonModel = (PlayerDeathReasonModel)b;

            //byte[] a = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, };

            //Span<byte> bytes = new Span<byte>(a);
            //bytes.Slice(2, 5);

            //int len = 0;
            //var b = new Span<byte>(new byte[1024], 0, 1024);

            //fixed (void* ptr_origi = b) {
            //    var ptr = ptr_origi;

            //    var packet = new ClientHello("Terraria183");

            //    packet.WriteContent(ref ptr);
            //    Console.WriteLine(string.Join(" ", (b.Slice(0, len)).ToArray().Select(b => $"{b:x2}")));


            //    var p = new WorldData(default, default, default, default, default, default, default, default, default, default, "", default, new byte[16], default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default);

            //    byte[] data = new byte[1024];
            //    var buffer = new Span<byte>(data);

            //    var sw = Stopwatch.StartNew();
            //    for (int i = 0; i < 10000000; ++i) {
            //        p.WriteContent(ref ptr);
            //        ptr = ptr_origi;
            //    }

            //    Console.WriteLine($"serialize cost: {sw.ElapsedMilliseconds}ms");

            //    p.WriteContent(ref ptr);
            //    ptr = ptr_origi;

            //    var size = (int)((long)ptr - (long)ptr_origi);

            //    sw = Stopwatch.StartNew();
            //    for (int i = 0; i < 10000000; ++i) {
            //        NetPacket.ReadNetPacket(ref ptr, size, false);
            //        ptr = ptr_origi;
            //    }

            //    Console.WriteLine($"deserialize cost: {sw.ElapsedMilliseconds}ms");

            //    Console.ReadLine();
            //}

            //byte[] arr = new byte[1024];

            //var sw = Stopwatch.StartNew();
            //fixed (void* ptr = arr) {
            //    for (int i = 0; i < testSize; ++i) {
            //        using var ms = new MemoryStream(arr);
            //        using var bw = new BinaryWriter(ms);
            //        bw.Write(text);
            //        ms.Position = 0;
            //    }
            //}
            //sw.Stop();
            //Console.WriteLine($"bw serialize cost: {sw.ElapsedMilliseconds}ms");

            //sw = Stopwatch.StartNew();
            //fixed (void* ptr = arr) {
            //    for (int i = 0; i < testSize; ++i) {
            //        var ptr_current = ptr;
            //        CommonCode.WriteString2(ref ptr_current, text);
            //        ptr_current = ptr;
            //    }
            //}
            //sw.Stop();
            //Console.WriteLine($"unsafe 2 serialize cost: {sw.ElapsedMilliseconds}ms");


            //sw = Stopwatch.StartNew();
            //fixed (void* ptr = arr) {
            //    for (int i = 0; i < testSize; ++i) {
            //        var ptr_current = ptr;
            //        CommonCode.WriteString(ref ptr_current, text);
            //        ptr_current = ptr;
            //    }
            //}
            //sw.Stop();
            //Console.WriteLine($"unsafe 1 serialize cost: {sw.ElapsedMilliseconds}ms");


            //using var ms2 = new MemoryStream(arr);
            //using var br = new BinaryReader(ms2);


            //sw = Stopwatch.StartNew();
            //for (int i = 0; i < testSize; ++i) {
            //    var str = br.ReadString();
            //    ms2.Position = 0;
            //}
            //sw.Stop();
            //Console.WriteLine($"br deserialize cost: {sw.ElapsedMilliseconds}ms");


            //sw = Stopwatch.StartNew();
            //fixed (void* ptr = arr) {
            //    for (int i = 0; i < testSize; ++i) {
            //        var ptr_current = ptr;
            //        var str = CommonCode.ReadString(ref ptr_current);
            //        ptr_current = ptr;
            //    }
            //}
            //sw.Stop();
            //Console.WriteLine($"unsafe 1 deserialize cost: {sw.ElapsedMilliseconds}ms");

            //sw = Stopwatch.StartNew();
            //fixed (void* ptr = arr) {
            //    for (int i = 0; i < testSize; ++i) {
            //        var ptr_current = ptr;
            //        var str = CommonCode.ReadString2(ref ptr_current);
            //        ptr_current = ptr;
            //    }
            //}
            //sw.Stop();
            //Console.WriteLine($"unsafe 2 deserialize cost: {sw.ElapsedMilliseconds}ms");
        }
    }
}