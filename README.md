**EnchCoreApi.TrProtocol**
===
### A efficient terraria packet serializer and partial protocol implement.
___
* This project automatically builds efficient and unsafe working code through IIncrementalGenerator.<br>
In addition, it provides an additional patch for OTAPI, by extracting some shared types to achieve friendly compatibility with OTAPI.<br>
Therefore, by using this specially designed OTAPI,
the data structures provided by the protocol library can be used directly in terraria's server programs.

* This project is based on the protocol data structures from another project by [chi-rei-den](https://github.com/chi-rei-den).
I have used their data structures as a reference and modified them according to my own needs. I would like to acknowledge and 
appreciate their work and contribution. You can find their original project at [TrProtocol](https://github.com/chi-rei-den/TrProtocol).

---
# Description of Terraria packet structure
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

# Application notice
### Serizialize

* Under normal circumstances, the protocol library does not need to know the packet header in serialization. 
Therefore, when using **'NetPacket.WriteContent (ref void\*)'**, the user only needs to pass in the pointer to the binary data that represents Type. 
This is the pointer at index=2 in the diagram table. 
The protocol library then writes the content of the packet to the pointer position and adds the offset that was written back to the ref void* pointer.
* So if you want to send a complete packet, your code should probarbly be written like this: [click here](#S2C_CombatTextInt)
### Deseriziable
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


# Development Expamle (Unfinished)

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
    //get the packet content size
    var size_short = (short)((long)ptr_current - (long)ptr); 

    //add packet header size to total size
    size_short += 2; 
    //write packet header value (total size)
    Unsafe.Write(ptr, size_short); 

    //send packet bytes
    socket.AsyncSend(SendBuffer, 0, size_short, delegate { }); 
}
```