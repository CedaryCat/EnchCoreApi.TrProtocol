namespace EnchCoreApi.TrProtocol.NugetBuilder
{
    internal class Program
    {
        static void Main(string[] args)
        {
            new NugetPackageBuilder("EnchCoreApi.TrProtocol.nupkg", "../../../../../docs/EnchCoreApi.TrProtocol.nuspec").Build();
        }
    }
}
