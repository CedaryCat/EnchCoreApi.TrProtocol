**EnchCoreApi.TrProtocol** 
[![GitHub Workflow](https://img.shields.io/github/workflow/status/CedaryCat/EnchCoreApi.TrProtocol/CI?logo=GitHub)](https://github.com/CedaryCat/EnchCoreApi.TrProtocol/actions) 
[![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/EnchCoreApi.TrProtocol?label=EnchCoreApi.TrProtocol)](https://www.nuget.org/packages/EnchCoreApi.TrProtocol) 
[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](https://www.gnu.org/licenses/gpl-3.0) 
===
### An efficient terraria packet serializer and partial protocol implement.
___
* This project automatically builds efficient and unsafe working code through IIncrementalGenerator.<br>
In addition, it provides an [additional patch for OTAPI](https://github.com/CedaryCat/EnchCoreApi.TrProtocol.OTAPI), by extracting some shared types to achieve friendly compatibility with OTAPI.<br>
Therefore, by using this specially [designed OTAPI](https://www.nuget.org/packages/EnchCoreApi.TrProtocol.OTAPI) ,
the data structures provided by the protocol library can be used directly in terraria's server programs.

* This project is based on the protocol data structures from another project by [chi-rei-den](https://github.com/chi-rei-den).
I have used their data structures as a reference and modified them according to my own needs. I would like to acknowledge and 
appreciate their work and contribution. You can find their original project at [TrProtocol](https://github.com/chi-rei-den/TrProtocol).

---

# Installation [![EnchCoreApi.TrProtocol](https://img.shields.io/nuget/vpre/EnchCoreApi.TrProtocol?label=EnchCoreApi.TrProtocol)](https://www.nuget.org/packages/EnchCoreApi.TrProtocol/)
* You can find and install it in nuget package manager, or you can install it directly from the nuget command line
```
PM> NuGet\Install-Package EnchCoreApi.TrProtocol -Version 1.0.3
```
* Please use version 1.0.2-beta1 or later version because 1.0.2-alpha1 or earlier version may resolves a bug that caused some fields to be serialized incorrectly due to missing conditionals.
# Usage
To use EnchCoreApi.TrProtocol, you need to add a reference to the namespace EnchCoreApi.TrProtocol, EnchCoreApi.TrProtocol.NetPackets, EnchCoreApi.TrProtocol.Models .etc 
```csharp
using EnchCoreApi.TrProtocol;
using EnchCoreApi.TrProtocol.Models;
using EnchCoreApi.TrProtocol.NetPackets;
```
```csharp
// create packet from given parammeters
var packet = new CombatTextInt(Vector2.Zero, Color.White, 100);
```
```csharp
// create packet from buffer
fixed (void* ptr = buffer) {
    var ptr_current = Unsafe.Add<byte>(ptr_current, offset);
    var packet = new CombatTextInt(ref ptr_current);
}
```

## Description of Terraria packet structure
### Before we proceed, let us first understand how Terraria handles packet receiving. 
* For the server, it uses a separate thread to run the **'Neplay.ServerLoop'** method, 
which calls the **'Neplay.UpdateConnectedClients'** method to constantly read data from 
each client's network stream and copy it to the corresponding buffer **'MessageBuffer.readBuffer'**.<br>
* In the main game loop, which updates at 60 frames per second, the **'Main.UpdateClientInMainThread'** 
method will access the buffer and process all the temporary data in one go. 
* Therefore, it is possible that the buffer contains multiple packets at a time. To solve the problem of 
packet fragmentation, Terraria will prepend a short value to each packet to indicate its size. We can call 
this value the packet header. Note that the short value also includes its own two-byte length.
### Example packet structure of CombatTextInt (81)
* this structure in the partocol libarry looks like this.
```csharp
public partial class CombatTextInt : NetPacket {

    public sealed override MessageID Type => MessageID.CombatTextInt;

    public Vector2 Position;
    public Color Color;
    public int Amount;
}
```
* this structure in binary looks like this.

<table>
    <tr> <!-- first row -->
        <th colspan="21">CombatTextInt (ID=81) </th> 
    </tr>
    <tr> <!-- second row -->
        <th colspan="3"> index </th>
        <th> 0 </th>
        <th> 1 </th>
        <th> 2 </th>
        <th> 3 </th>
        <th> 4 </th>
        <th> 5 </th>
        <th> 6 </th>
        <th> 7 </th>
        <th> 8 </th>
        <th> 9 </th>
        <th> 10 </th>
        <th> 11 </th>
        <th> 12 </th>
        <th> 13 </th>
        <th> 14 </th>
        <th> 15 </th>
        <th> 16 </th>
        <th> 17 </th>
    </tr>
    <tr>
        <th colspan="3" rowspan="2"> name </th>
        <th colspan="2" rowspan="2"> packet<br>header </th>
        <th colspan="16"> packet content (Size=16)</th>
    </tr>
    <tr> <!-- third row -->
        <th> Type </th>
        <th colspan="8"> Position </th>
        <th colspan="3"> Color </th>
        <th colspan="4"> Amount </th>
    </tr>
    <tr>
        <th colspan="3"> field type </th>
        <th colspan="2"> short </th>
        <th> MessageID </th>
        <th colspan="8"> Vector2 </th>
        <th colspan="3"> Color </th>
        <th colspan="4"> int32 </th>
    </tr>
    <tr>
        <th colspan="3"> real type </th>
        <th colspan="2"> short </th>
        <th> byte </th>
        <th colspan="4"> float </th>
        <th colspan="4"> float </th>
        <th> byte </th>
        <th> byte </th>
        <th> byte </th>
        <th colspan="4"> int32 </th>
    </tr>
    <tr>
        <th colspan="3"> value </th>
        <th colspan="2"> 18 </th>
        <th> 81 </th>
        <th colspan="4"> X </th>
        <th colspan="4"> Y </th>
        <th> R </th>
        <th> G </th>
        <th> B </th>
        <th colspan="4"> num </th>
    </tr>
</table>

## **Note**
### Serialize

* Under normal circumstances, the protocol library does not need to know the packet header in serialization. 
Therefore, when using **'NetPacket.WriteContent (ref void\*)'**, the user only needs to pass in the pointer to the binary data that represents Type. 
This is the pointer at index=2 in the diagram table. 
The protocol library then writes the content of the packet to the pointer position and adds the offset that was written back to the ref void* pointer.
* So if you want to send a complete packet, your code should probarbly be written like this: [click here](#S2C_CombatTextInt)
### Deserialize
* In deserialization, however, there is a notable problem. In Terraria, some packets need to be resolved based on the state of the game, 
such as the **'NetCreativePowersModule'** packet. This packet is used to synchronize the creative powers of the players in the game. In Terraria 
server, it calls the function **'APerPlayerTogglePower.Deserialize_SyncEveryone'**, which has the following code:
```csharp
// Terraria.GameContent.Creative.CreativePowers.APerPlayerTogglePower

public void Deserialize_SyncEveryone(BinaryReader reader, int userId) {
    int num = (int)Math.Ceiling((float)_perPlayerIsEnabled.Length / 8f);
    if (Main.netMode == 2 && !CreativePowersHelper.IsAvailableForPlayer(this, userId)) {
        reader.ReadBytes(num);
        return;
    }
    for (int i = 0; i < num; i++) {
        BitsByte bitsByte = reader.ReadByte();
        for (int j = 0; j < 8; j++) {
            int num2 = i * 8 + j;
            if (num2 != Main.myPlayer) {
                if (num2 >= _perPlayerIsEnabled.Length)
                    break;
                SetEnabledState(num2, bitsByte[j]);
            }
        }
    }
}
```
* The condition **'CreativePowersHelper.IsAvailableForPlayer(this, userId)'** shows that the resolution of this packet depends on the player 
who sent it. However, the protocol library that handles the packets is stateless, meaning it does not keep track of the game state or the players. 
Therefore, it cannot handle such data properly. 

* To solve this problem, the protocol library implements the **IExtraData** interface for packets that have similar properties. This interface contains 
an **'ExtraData:byte[]'** Property where unprocessed data at the end of the packet is stored. The users of the protocol library can then handle this 
data themselves according to their needs. 

* Because of this, during packet deserialization, the protocol library must know the packet length in order to properly transfer the remaining data 
that cannot be processed to ExtraData. Therefore, the second argument of **'NetPacket.ReadNetPacket(ref void\*, int restContentSize, bool isServerSide)'** 
should be filled with **packetContentSize**, which is the value of the **packet header** minus **2**.

# Expamles (Unfinished)

<span id="S2C_CombatTextInt"></span>
### 1. Server send a CombatTextInt to player which whoAmI = 0
```csharp
//socket be sended to
var socket = Netplay.Clients[0].Socket; 
//create a packet
var packet = new CombatTextInt(Vector2.Zero, Color.White, 100); 

//get a pointer to buffer index = 0
fixed (void* ptr = SendBuffer) { 
    //skip the packet header
    var ptr_current = Unsafe.Add<short>(ptr, 1); 
    //write packet
    packet.WriteContent(ref ptr_current); 
    //get the packet total size (including 2 bytes of packet header)
    var size_short = (short)((long)ptr_current - (long)ptr); 
    //write packet header value
    Unsafe.Write(ptr, size_short); 

    //send packet bytes
    socket.AsyncSend(SendBuffer, 0, size_short, delegate { }); 
}
```
# Profermance [![GitHub Workflow](https://img.shields.io/badge/Source-Github-d021d6?style=flat&logo=GitHub)](https://github.com/CedaryCat/EnchCoreApi.TrProtocol/blob/master/src/EnchCoreApi.TrProtocol.Test.Performance/PacketPerformanceTest.cs) 
## Take the packet **WorldData (ID=7)** as an example
* **Note:** skip offset0 because you already know what kind of package it is.
### Serialize 
```
[Benchmark] public unsafe void Test_Unsafe() {
    fixed (void* ptr = buffer) {
        var p = Unsafe.Add<byte>(ptr, 1); // skip offset0
        worldData.ReadContent(ref p);
    }
}
```
```
[Benchmark] public void Test_BinaryWriter() {
    var bw = new BinaryWriter(new MemoryStream(buffer));
    bw.BaseStream.Position = 1; // skip offset0
    bw.Write(worldData.Time);
    //...
}
```
```
[Benchmark] public void Test_ReuseBinaryWriter() {
    bw.BaseStream.Position = 1; // skip offset0
    bw.Write(worldData.Time);
    //...
}
```
* **Result**

|                 Method |      Mean |    Error |   StdDev | Rank |   Gen0 | Allocated |
|----------------------- |:----------:|:---------:|:---------:|:-----:|:-------:|:----------:|
|            Test_Unsafe |  95.44 ns | 0.362 ns | 0.321 ns |    1 | 0.0086 |      72 B |
|      Test_BinaryWriter | 339.91 ns | 0.863 ns | 0.720 ns |    3 | 0.0124 |     104 B |
| Test_ReuseBinaryWriter | 326.54 ns | 0.950 ns | 0.742 ns |    2 |      - |         - |

---
### Deserialize
```
[Benchmark] public unsafe void Test_Unsafe() {
    fixed (void* ptr = buffer) {
        var p = Unsafe.Add<byte>(ptr, 1); // skip offset0
        worldData.ReadContent(ref p);
    }
}
```
```
[Benchmark] public void Test_BinaryReader() {
    var br = new BinaryReader(new MemoryStream(buffer));
    br.BaseStream.Position = 1; // skip offset0
    worldData.Time = br.ReadInt32();
    //...
}
```
```
[Benchmark] public void Test_ReuseBinaryReader() {
    br.BaseStream.Position = 1; // skip offset0
    worldData.Time = br.ReadInt32();
    //...
}
```
* **Result**

|                 Method |      Mean |    Error |   StdDev | Rank |   Gen0 | Allocated |
|----------------------- |:----------:|:---------:|:---------:|:-----:|:-------:|:----------:|
|            Test_Unsafe |  95.78 ns | 0.713 ns | 0.667 ns |    1 | 0.0086 |      72 B |
|      Test_BinaryReader | 313.89 ns | 6.125 ns | 8.587 ns |    3 | 0.0877 |     736 B |
| Test_ReuseBinaryReader | 229.80 ns | 0.564 ns | 0.500 ns |    2 | 0.0086 |      72 B |

---
## The others
* The performance of string serialization/deserialization has been improved by about 20%. Due to space constraints, I will not discuss the details here. If you are interested, you can visit [this link](https://github.com/CedaryCat/EnchCoreApi.TrProtocol/blob/master/src/EnchCoreApi.TrProtocol.Test.Performance/StringPerformanceTest.cs) to see more.

# RoadMap
### Planned feature
---
* [ ] **Simplify packet construction**
    * To simplify the packet construction, we plan to provide default values for the assignment parameters of the fields that are not required. 
However, this will change the order of the arguments, since the parameters with default values must be placed at the end of the constructor. 
To maintain backward compatibility with older versions of the API, we will generate a new constructor overload with the adjusted parameter order 
and keep the old constructor as well.
---
* [ ] **XML annotation for construction**
    * We plan to add an XML annotation to the constructor of each packet that receives the initialization content from the pointer. 
This annotation will remind the user how to use it correctly. Such constructors are usually used by the protocol library, 
and they should not be used by the user unless they know exactly what they are doing. Otherwise, the user should use another method of reading the packet from the pointer, 
such as **'NetPacket.ReadNetPacket'**.

