

//BenchmarkRunner.Run<StringPerformanceTest.StringTestWrite>(ManualConfig.Create(DefaultConfig.Instance).WithOptions(ConfigOptions.DisableOptimizationsValidator));
//BenchmarkRunner.Run<StringPerformanceTest.StringTestRead>(ManualConfig.Create(DefaultConfig.Instance).WithOptions(ConfigOptions.DisableOptimizationsValidator));

BenchmarkRunner.Run<PacketPerformanceTest.Read>(ManualConfig.Create(DefaultConfig.Instance).WithOptions(ConfigOptions.DisableOptimizationsValidator));
BenchmarkRunner.Run<PacketPerformanceTest.Write>(ManualConfig.Create(DefaultConfig.Instance).WithOptions(ConfigOptions.DisableOptimizationsValidator));

Console.ReadLine();
