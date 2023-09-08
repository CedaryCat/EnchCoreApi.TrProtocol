using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using EnchCoreApi.TrProtocol;

namespace SerializeTest {
    internal class Program {
        static unsafe void Main(string[] args) {

            //var buffer = new byte[1024];
            //var memory = new MemoryStream(buffer);
            //var br = new BinaryReader(memory);
            //var bw = new BinaryWriter(memory);
            //bw.Write("???????????????????????????");
            //memory.Position = 0;

            //for (int i = 0; i < 1000000000; i++)
            //{
            //    fixed (void* ptr = buffer)
            //    {
            //        var p = ptr;
            //        CommonCode.WriteString(ref p, "??????????????????????");
            //    }
            //}

            BenchmarkRunner.Run<StringPerformanceTest.StringTestWrite>(ManualConfig.Create(DefaultConfig.Instance).WithOptions(ConfigOptions.DisableOptimizationsValidator));
            //BenchmarkRunner.Run<StringPerformanceTest.StringTestRead>(ManualConfig.Create(DefaultConfig.Instance).WithOptions(ConfigOptions.DisableOptimizationsValidator));
            Console.ReadLine();
        }
    }
}