using EnchCoreApi.TrProtocol.Interfaces;
using System.Runtime.InteropServices;

namespace EnchCoreApi.TrProtocol.Models;

[StructLayout(LayoutKind.Sequential)]
public partial struct Buff : ISoildSerializableData {
    public ushort BuffType;
    public short BuffTime;
}
