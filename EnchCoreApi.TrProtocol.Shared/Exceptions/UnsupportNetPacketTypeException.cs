using System;
using System.Collections.Generic;
using System.Text;

namespace EnchCoreApi.TrProtocol.Exceptions
{
    public class UnsupportNetPacketTypeException : Exception {
        public UnsupportNetPacketTypeException(Type packetbase, Enum id, long value) : base ($"id '{id}' ({value}) of base packet '{packetbase}' is not defined") {

        }
    }
}
