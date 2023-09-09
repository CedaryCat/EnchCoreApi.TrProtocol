using ModFramework;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Reflection;

namespace EnchCoreApi.TrProtocol.Patcher
{

    [MonoMod.MonoModIgnore]
    public static partial class Common
    {

        public static string GetCliValue(string key)
        {
            string find = $"-{key}=";
            var match = Array.Find(Environment.GetCommandLineArgs(), x => x.StartsWith(find, StringComparison.CurrentCultureIgnoreCase));
            return match?.Substring(find.Length)?.ToLower();
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
    }
}
