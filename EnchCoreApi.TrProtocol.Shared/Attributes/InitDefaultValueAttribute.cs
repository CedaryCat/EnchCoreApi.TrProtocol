using System;
using System.Collections.Generic;
using System.Text;

namespace EnchCoreApi.TrProtocol.Attributes {
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class InitDefaultValueAttribute : Attribute {
    }
}
