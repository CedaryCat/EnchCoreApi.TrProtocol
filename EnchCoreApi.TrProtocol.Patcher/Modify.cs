using Mono.Cecil;
using Assembly = System.Reflection.Assembly;

namespace EnchCoreApi.TrProtocol.Patcher {
    public abstract class Modify {
        protected readonly AssemblyDefinition destination;
        protected readonly Assembly modelsAssembly;
        public Modify(AssemblyDefinition destination, Assembly assembly) {
            this.destination = destination;
            modelsAssembly = assembly;

        }
        public abstract string Name { get; }
        public abstract void Run(Logger logger);
    }
}
