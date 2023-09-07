using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnchCoreApi.TrProtocol.Models.Interfaces {
    public interface ISerializableData<TSequentialLayout> where TSequentialLayout : unmanaged {
        TSequentialLayout SequentialData { get; set; }
    }
}
