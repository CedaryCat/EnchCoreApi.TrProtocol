using EnchCoreApi.TrProtocol.Attributes;
using Mono.Cecil;
using MonoMod.Utils;

namespace EnchCoreApi.TrProtocol.Patcher
{
    [MonoMod.MonoModIgnore]
    internal class Program {
        static void Main(string[] args)
        {
            new Patcher().Patch();
        }
    }
}