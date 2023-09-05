using Mono.Cecil;
using System.Reflection;

namespace EnchCoreApi.TrProtocol.Patcher.CecilTool {
    public static class ParameterTypeExtensions {
        public static string ToParamString(this IEnumerable<ParameterType> collection) {
            return "(" + string.Join(",", collection.Select((ParameterType x) => x.TypeName)) + ")";
        }

        public static IEnumerable<ParameterType> ToParameters(this IEnumerable<ParameterDefinition> collection) {
            foreach (ParameterDefinition item in collection) {
                yield return ParameterType.From(item);
            }
        }

        public static IEnumerable<ParameterType> ToParameters(this IEnumerable<ParameterInfo> collection) {
            foreach (ParameterInfo item in collection) {
                yield return ParameterType.From(item);
            }
        }
    }
}
