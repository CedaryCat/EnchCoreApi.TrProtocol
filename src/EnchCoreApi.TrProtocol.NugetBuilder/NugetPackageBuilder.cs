using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EnchCoreApi.TrProtocol.NugetBuilder
{
    public class NugetPackageBuilder
    {
        string GetNugetVersionFromAssembly(Assembly assembly)
            => assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;

        string GetNugetVersionFromAssembly<TType>()
            => GetNugetVersionFromAssembly(typeof(TType).Assembly);

        public string PackageName { get; set; }
        public string NuspecPath { get; set; }

        public NugetPackageBuilder(string packageName, string nuspecPath)
        {
            PackageName = packageName;
            NuspecPath = nuspecPath;
        }

        public static string GetGitCommitSha()
        {
            var commitSha = Environment.GetEnvironmentVariable("GITHUB_SHA")?.Trim();
            if (commitSha != null && commitSha.Length >= 7)
            {
                return commitSha.Substring(0, 7);
            }
            return null;
        }

        public void Build()
        {
            var nuspec_xml = File.ReadAllText(NuspecPath);
            nuspec_xml = nuspec_xml.Replace("[INJECT_VERSION]", GetNugetVersionFromAssembly<MessageID>());

            var commitSha = GetGitCommitSha();
            nuspec_xml = nuspec_xml.Replace("[INJECT_GIT_HASH]", String.IsNullOrWhiteSpace(commitSha) ? "" : $" git#{commitSha}");

            var platforms = new[] { "net6.0" };
            var xml_dependency = "";
            var xml_group = String.Join("", platforms.Select(platform => $"\n\t<group targetFramework=\"{platform}\">{xml_dependency}\n\t</group>"));
            var xml_dependencies = $"<dependencies>{xml_group}\n    </dependencies>";

            nuspec_xml = nuspec_xml.Replace("[INJECT_DEPENDENCIES]", xml_dependencies);

            nuspec_xml = nuspec_xml.Replace("[INJECT_YEAR]", DateTime.UtcNow.Year.ToString());

            using (var nuspec = new MemoryStream(Encoding.UTF8.GetBytes(nuspec_xml)))
            {
                var manifest = NuGet.Packaging.Manifest.ReadFrom(nuspec, validateSchema: true);
                var packageBuilder = new NuGet.Packaging.PackageBuilder();
                packageBuilder.Populate(manifest.Metadata);

                packageBuilder.AddFiles("../../../../../", "COPYING.txt", "COPYING.txt");

                foreach (var platform in platforms)
                {
                    var dest = Path.Combine("lib", platform);
                    packageBuilder.AddFiles(Environment.CurrentDirectory, "EnchCoreApi.TrProtocol.dll", dest);
                }

                if (File.Exists(PackageName))
                    File.Delete(PackageName);

                using (var srm = File.OpenWrite(PackageName))
                    packageBuilder.Save(srm);
            }
        }
    }
}
