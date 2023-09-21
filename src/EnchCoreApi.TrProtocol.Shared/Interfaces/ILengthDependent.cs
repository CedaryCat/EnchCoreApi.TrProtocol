using System;
using System.Collections.Generic;
using System.Text;

namespace EnchCoreApi.TrProtocol.Interfaces
{
    public interface ILengthDependent
    {
        unsafe void ReadContent(ref void* ptr, void* end_ptr);
        unsafe void ReadContent(ref void* ptr, int restContentLen);
        unsafe void WriteContent(ref void* ptr);
    }
}
