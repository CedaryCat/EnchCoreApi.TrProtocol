using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnchCoreApi.TrProtocol.Attributes;
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class CastOperatorPlaceHolderAttribute : Attribute {
    public CastOperatorPlaceHolderAttribute() {
    }
}
