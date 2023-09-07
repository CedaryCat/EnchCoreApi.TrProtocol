using EnchCoreApi.TrProtocol.Attributes;
using EnchCoreApi.TrProtocol.Patcher.CecilTool;
using ILRepacking;
using Mono.Cecil;
using MonoMod.Utils;

namespace EnchCoreApi.TrProtocol.Patcher {
    internal class Program {
        static void Main(string[] args) {
            var logger = new ConsoleLogger();

            var protocolAssembly = typeof(MessageID).Assembly;
            var protocolFile = new FileInfo(protocolAssembly.Location);

            var otapiFile = new FileInfo(typeof(Terraria.Main).Assembly.Location);
            var convFile = new FileInfo(typeof(Convertion.Convertion).Assembly.Location);


            var dir = Directory.CreateDirectory(Path.Combine(new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName, "Output"));
            var mirgratedOTAPIPath = Path.Combine(dir.FullName, "OTAPI.Mirgrated.dll");
            var mergiedOTAPIPath = Path.Combine(dir.FullName, "OTAPI.Mergied.dll");
            var finalOTAPIPath = Path.Combine(dir.FullName, "OTAPI.dll");






            var otapi = AssemblyDefinition.ReadAssembly(otapiFile.FullName);
            var forwardMod = new TypeMigrationModify(otapi, protocolAssembly);
            forwardMod.Run(logger);
            otapi.Write(mirgratedOTAPIPath);

            logger.WriteLine("<<<<<<<<<<=====-=-=====o=0=o=====-=-=====>>>>>>>>>>");
            logger.WriteLine($"类型导出完毕，以下为成功导出类型");
            foreach (var t in forwardMod.Forwardeds) {
                logger.WriteLine($"Exported::{t}");
            }
            logger.WriteLine("<<<<<<<<<<=====-=-====='=0='=====-=-=====>>>>>>>>>>");
            logger.WriteLine($"开始合并Custom EasyCast");





            ILRepack ilRepacket = new ILRepack(new RepackOptions() { 
                SearchDirectories = new string[] { },
                InputAssemblies = new string[] {
                    mirgratedOTAPIPath,
                    convFile.FullName,
                },
                UnionMerge = true,
                OutputFile = mergiedOTAPIPath,
                TargetKind = ILRepack.Kind.Dll,
                CopyAttributes = true,
            });
            ilRepacket.Repack();





            logger.WriteLine($"开始创建EasyCast方法");
            otapi = AssemblyDefinition.ReadAssembly(mergiedOTAPIPath);
            var castMod = new EasyCastModify(otapi, protocolAssembly);
            castMod.Run(logger);


            logger.WriteLine($"完成，正在写入文件：{finalOTAPIPath}");
            otapi.Write(finalOTAPIPath);
        }
    }
}