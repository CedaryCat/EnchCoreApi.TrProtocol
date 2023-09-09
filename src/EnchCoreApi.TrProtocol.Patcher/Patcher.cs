using ModFramework.Modules.CSharp;
using ModFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using ModFramework.Relinker;
using static ModFramework.ModContext;
using Mono.Cecil;

namespace EnchCoreApi.TrProtocol.Patcher
{
    [MonoMod.MonoModIgnore]
    public class Patcher
    {
        public ModContext ModContext { get; } = new("Terraria");

        public virtual string DisplayText => "OTAPI PC Server - Protocol Compatible";

        public virtual string InstallDestination { get; } = Environment.CurrentDirectory;

        public virtual string OutputDirectory => Path.Combine(ModContext.BaseDirectory, "outputs");

        public virtual NugetPackageBuilder NugetPackager { get; } = new("EnchCoreApi.OTAPI.ProtoCompatible.nupkg", "../../../../../docs/EnchCoreApi.OTAPI.ProtoCompatible.nuspec");

        public virtual string ArtifactName { get; } = "artifact-protocol";
        public virtual bool GenerateSymbols => true;

        public virtual string GetEmbeddedResourcesDirectory(string fileinput)
        {
            return Path.Combine(ModContext.BaseDirectory, Path.GetDirectoryName(fileinput));
        }

        public void Patch()
        {
            try
            {
                var input = typeof(Main).Assembly.Location;
                var temp = Path.Combine(ModContext.BaseDirectory, "OTAPI.temp.dll");
                var version = typeof(Main).Assembly
                    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                    ?.InformationalVersion ?? throw new NullReferenceException();
                var inputName = Path.GetFileNameWithoutExtension(input);

                Console.WriteLine("[OTAPI] Extracting embedded binaries and packing into one binary...");
                var embeddedResources = this.ExtractResources(input);

                Directory.CreateDirectory(OutputDirectory);

                //var refs = Path.Combine(basepath, "TerrariaServer.dll");
                var otapi = Path.Combine(OutputDirectory, "OTAPI.dll");
                var hooks = Path.Combine(OutputDirectory, "OTAPI.Runtime.dll");

                ModContext.ReferenceFiles.AddRange(new[]
                {
                    "ModFramework.dll",
                    "MonoMod.dll",
                    "MonoMod.Utils.dll",
                    "MonoMod.RuntimeDetour.dll",
                    "Mono.Cecil.dll",
                    "Mono.Cecil.Rocks.dll",
                    "Newtonsoft.Json.dll",
                    "Steamworks.NET.dll",
                    typeof(MessageID).Assembly.Location,
                });

                //ModContext.ReferenceFiles.Add(refs);

                // load into the current app domain for patch refs
                //var asm = Assembly.LoadFile(refs);
                var cache = new Dictionary<string, Assembly>();

                Assembly? ResolvingFile(AssemblyLoadContext arg1, AssemblyName args)
                {
                    if (args.Name is null) return null;
                    if (cache.TryGetValue(args.Name, out Assembly? asmm))
                        return asmm;

                    //if (args.Name == asm.GetName().Name) //.IndexOf("TerrariaServer") > -1)
                    //{
                    //    cache[args.Name] = asm;
                    //    return asm;
                    //}

                    var dll = $"{args.Name}.dll";
                    if (File.Exists(dll))
                    {
                        asmm = Assembly.Load(File.ReadAllBytes(dll));
                        cache[args.Name] = asmm;
                        return asmm;
                    }

                    return null;
                }

                AssemblyLoadContext.Default.Resolving += ResolvingFile;
                try
                {
                    this.Patch("Applying types migration and custom convertion", input, temp, false, (modType, modder) =>
                    {
                        if (modder is not null)
                        {
                            if (modType == ModType.PreRead)
                            {
                                modder.AssemblyResolver.AddSearchDirectory(embeddedResources);
                            }
                            else if (modType == ModType.Read)
                            {
                                var logger = new ConsoleLogger();

                                var typeMigration = new TypeMigrationModify(modder, typeof(MessageID).Assembly);
                                typeMigration.Run(logger);
                                logger.WriteLine($"类型导出完毕，以下为成功导出类型");
                                foreach (var t in typeMigration.Forwardeds)
                                    logger.WriteLine($"Exported::{t}");


                                // merge custom convertion
                                modder.ReadMod(typeof(Convertion.Convertion).Assembly.Location);

                            }
                        }
                        return EApplyResult.Continue;
                    }); 

                    this.Patch("Add Easy Cast", temp, otapi, false, (modType, modder) =>
                    {
                        if (modder is not null)
                        {
                            if (modType == ModType.PreRead)
                            {
                                modder.AssemblyResolver.AddSearchDirectory(embeddedResources);
                            }
                            else if (modType == ModType.Read)
                            {
                                var logger = new ConsoleLogger();
                                var easycastModify = new EasyCastModify(modder, typeof(MessageID).Assembly);
                                easycastModify.Run(logger);
                            }
                            else if (modType == ModType.PreWrite)
                            {
                                modder.ModContext.TargetAssemblyName = "OTAPI"; // change the target assembly since otapi is now valid for write events
                            }
                            else if (modType == ModType.Write)
                            {
                                modder.ModContext = new("OTAPI.Runtime"); // done with the context. they will be triggered again by runtime hooks if not for this
                                CreateRuntimeHooks(modder, hooks);

                                Console.WriteLine("[OTAPI] Building NuGet package...");
                                NugetPackager.Build(modder, version, OutputDirectory);
                            }
                        }
                        return EApplyResult.Continue;
                    });
                }
                finally
                {
                    AssemblyLoadContext.Default.Resolving -= ResolvingFile;
                }
            }
            finally
            {
            }

            Console.WriteLine("[OTAPI] Building artifacts...");
            WriteCIArtifacts(ArtifactName);

            Console.WriteLine("Patching has completed.");
        }

        public virtual void AddSearchDirectories(ModFwModder modder) { }

        public string ExtractResources(string fileinput)
        {
            var dir = GetEmbeddedResourcesDirectory(fileinput);
            var extractor = new ResourceExtractor();
            var embeddedResourcesDir = extractor.Extract(fileinput, dir);
            return embeddedResourcesDir;
        }

        public void WriteCIArtifacts(string outputFolder)
        {
            if (Directory.Exists(outputFolder)) Directory.Delete(outputFolder, true);
            Directory.CreateDirectory(outputFolder);

            File.Copy("../../../../../COPYING.txt", Path.Combine(outputFolder, "COPYING.txt"));
            File.Copy(Path.Combine(OutputDirectory, "OTAPI.dll"), Path.Combine(outputFolder, "OTAPI.dll"));
            File.Copy(Path.Combine(OutputDirectory, "OTAPI.Runtime.dll"), Path.Combine(outputFolder, "OTAPI.Runtime.dll"));
        }

        public string Patch(string status, string input, string output, bool publicEverything,
            Func<ModType, ModFwModder?, EApplyResult> onApplying,
            Action<ModFwModder, string>? print = null
        )
        {
            if (print is null) print = (_, str) => Console.WriteLine(str);

            EApplyResult OnApplying(ModType modType, ModFwModder? modder)
            {
                return onApplying(modType, modder);
            };

            ModContext.OnApply += OnApplying;
            try
            {
                using ModFwModder mm = new(ModContext)
                {
                    InputPath = input,
                    OutputPath = output,
                    MissingDependencyThrow = false,
                    PublicEverything = publicEverything,

                    LogVerboseEnabled = true,

                    GACPaths = new string[] { }, // avoid MonoMod looking up the GAC, which causes an exception on .netcore

                    //EnableWriteEvents = writeEvents,
                };

                AddSearchDirectories(mm);
                mm.AddTask<CoreLibRelinker>();

                mm.Read();

                print(mm, $"[OTAPI] Mapping dependencies: {status}");
                mm.MapDependencies();

                print(mm, $"[OTAPI] Patching: {status}");
                mm.AutoPatch();

                print(mm, $"[OTAPI] Writing: {status}, Path={new Uri(Environment.CurrentDirectory).MakeRelativeUri(new(mm.OutputPath))}");

                if (!GenerateSymbols)
                {
                    mm.WriterParameters.SymbolWriterProvider = null;
                    mm.WriterParameters.WriteSymbols = false;
                }

                mm.Write();

                return mm.OutputPath;
            }
            finally
            {
                ModContext.OnApply -= OnApplying;
            }
        }
        public static void CreateRuntimeHooks(ModFwModder modder, string output)
        {
            modder.Log("[OTAPI] Generating OTAPI.Runtime.dll");
            var gen = new MonoMod.RuntimeDetour.HookGen.HookGenerator(modder, "OTAPI.Runtime.dll");
            using var srm = new MemoryStream();
            using (ModuleDefinition mOut = gen.OutputModule)
            {
                gen.Generate();

                mOut.Write(srm);
            }

            srm.Position = 0;
            var fileName = Path.GetFileName(output);
            using var mm = new ModFwModder(new("OTAPI.Runtime"))
            {
                Input = srm,
                OutputPath = output,
                MissingDependencyThrow = false,
                //LogVerboseEnabled = true,
                // PublicEverything = true, // this is done in setup

                GACPaths = new string[] { } // avoid MonoMod looking up the GAC, which causes an exception on .netcore
            };
            mm.Log($"[OTAPI] Processing corelibs to be net6: {fileName}");

            mm.Read();

            mm.AddTask<CoreLibRelinker>();

            mm.MapDependencies();
            mm.AutoPatch();

            mm.Write();
        }
    }
}
