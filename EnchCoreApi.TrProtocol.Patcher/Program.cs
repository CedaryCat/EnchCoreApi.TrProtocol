using EnchCoreApi.TrProtocol.Attributes;
using EnchCoreApi.TrProtocol.Patcher.CecilTool;
using Mono.Cecil;
using MonoMod.Utils;

namespace EnchCoreApi.TrProtocol.Patcher {
    internal class Program {
        static void Main(string[] args) {
            var modelsAssembly = typeof(MessageID).Assembly;
            #region
            var otapi = AssemblyDefinition.ReadAssembly(typeof(Terraria.Main).Assembly.Location);

            var logger = new ConsoleLogger();
            var forwardMod = new TypeMigrationModify(otapi, modelsAssembly);
            var castMod = new EasyCastModify(otapi, modelsAssembly);

            forwardMod.Run(logger);

            logger.WriteLine("<<<<<<<<<<=====-=-=====o=0=o=====-=-=====>>>>>>>>>>");
            logger.WriteLine($"类型导出完毕，以下为成功导出类型");
            foreach (var t in forwardMod.Forwardeds) {
                logger.WriteLine($"Exported::{t}");
            }
            logger.WriteLine("<<<<<<<<<<=====-=-====='=0='=====-=-=====>>>>>>>>>>");
            logger.WriteLine($"开始创建EasyCast方法");

            castMod.Run(logger);

            var dir = Directory.CreateDirectory(Path.Combine(new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName, "Output"));
            var path = Path.Combine(dir.FullName, "OTAPI.dll");
            logger.WriteLine($"完成，正在写入文件：{path}");
            otapi.Write(path);
            #endregion
        }
    }
}