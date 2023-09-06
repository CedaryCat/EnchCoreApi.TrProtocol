using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnchCoreApi.TrProtocol.Attributes {
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class CastOperatorPlaceHolderAttribute : Attribute {
        public readonly CastOperator CastOperator;
        public CastOperatorPlaceHolderAttribute(CastOperator castOperator) {
            CastOperator = castOperator;
        }
    }
    public enum CastOperator {
        Implicit,
        Explicit,
    }
}
