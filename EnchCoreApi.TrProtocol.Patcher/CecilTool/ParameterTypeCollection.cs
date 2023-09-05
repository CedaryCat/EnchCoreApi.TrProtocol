using Mono.Cecil;
using Mono.Collections.Generic;
using System.Reflection;

namespace EnchCoreApi.TrProtocol.Patcher.CecilTool {
    public class ParameterTypeCollection : List<ParameterType> {
        public static explicit operator ParameterTypeCollection(Collection<ParameterDefinition> collection) {
            ParameterTypeCollection parameterTypeCollection = new ParameterTypeCollection();
            foreach (ParameterDefinition item in collection) {
                parameterTypeCollection.Add(ParameterType.From(item));
            }

            return parameterTypeCollection;
        }

        public static explicit operator ParameterTypeCollection(ParameterInfo[] collection) {
            ParameterTypeCollection parameterTypeCollection = new ParameterTypeCollection();
            foreach (ParameterInfo parameter in collection) {
                parameterTypeCollection.Add(ParameterType.From(parameter));
            }

            return parameterTypeCollection;
        }
    }
}
