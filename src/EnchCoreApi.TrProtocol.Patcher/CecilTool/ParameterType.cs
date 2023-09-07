using Mono.Cecil;
using System.Reflection;

namespace EnchCoreApi.TrProtocol.Patcher.CecilTool {
    public struct ParameterType {
        public string Name { get; set; }

        public string TypeName { get; set; }

        public object Type { get; set; }

        public static ParameterType From(TypeReference type) {
            ParameterType result = default(ParameterType);
            result.Name = type.Name;
            result.TypeName = type.FullName;
            result.Type = type;
            return result;
        }

        public static ParameterType From(ParameterDefinition parameter) {
            ParameterType result = default(ParameterType);
            result.Name = parameter.ParameterType.Name;
            result.TypeName = parameter.ParameterType.FullName;
            result.Type = parameter.ParameterType;
            return result;
        }

        public static ParameterType From(ParameterInfo parameter) {
            ParameterType result = default(ParameterType);
            result.Name = parameter.ParameterType.Name;
            result.TypeName = parameter.ParameterType.FullName;
            result.Type = parameter.ParameterType;
            return result;
        }
    }
}
