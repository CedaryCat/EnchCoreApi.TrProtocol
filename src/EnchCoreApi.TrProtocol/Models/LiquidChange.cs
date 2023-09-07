using EnchCoreApi.TrProtocol.Interfaces;
using System.Runtime.InteropServices;
using Terraria.DataStructures;

namespace EnchCoreApi.TrProtocol.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct LiquidChange : ISoildSerializableData {
    public Point16 Position;
    public byte LiquidAmount;
    public LiquidType LiquidType;
}
