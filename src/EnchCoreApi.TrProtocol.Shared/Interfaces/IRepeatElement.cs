using System;
using System.Collections.Generic;
using System.Text;

namespace EnchCoreApi.TrProtocol.Interfaces
{
    public interface IRepeatElement<TCount> : ISerializableData where TCount : unmanaged, IConvertible
    {
        public TCount RepeatCount { get; set; }
    }
}
