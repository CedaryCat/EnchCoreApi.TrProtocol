using EnchCoreApi.TrProtocol.Models;
using EnchCoreApi.TrProtocol.NetPackets;
using EnchCoreApi.TrProtocol.NetPackets.Modules;
using System.Runtime.CompilerServices;

namespace EnchCoreApi.TrProtocol.Test
{
    [TestClass]
    public class NetPacketTest
    {
        [TestMethod]
        public unsafe void ClientHello()
        {
            var hello = new ClientHello("Terraria");

            byte[] buffer1 = new byte[1024];
            using var bw = new BinaryWriter(new MemoryStream(buffer1));
            bw.Write((byte)hello.Type);
            bw.Write(hello.Version);

            byte[] buffer2 = new byte[1024];
            fixed (void* ptr = buffer2)
            {
                var p = ptr;
                hello.WriteContent(ref p);
            }

            for (int i = 0; i < bw.BaseStream.Position; i++)
            {
                Assert.AreEqual(buffer1[i], buffer2[i]);
            }
        }
        byte[] NetCreativePowersModuleData = new byte[] { 
            (byte)MessageID.NetModules, //net packet id
            (byte)NetModuleType.NetCreativePowersModule, 0, //net module id
            (byte)CreativePowerTypes.Godmode, 0, //field of PowerType

            0, 1, 2, 3, 4 // rest data
        };
        [TestMethod]
        public unsafe void NetCreativePowersModule()
        {
            NetCreativePowersModule power;
            fixed (void* ptr = NetCreativePowersModuleData)
            {
                var p = ptr;
                power = (NetCreativePowersModule)NetPacket.ReadNetPacket(ref p, 1 + 2 + 2 + 5, true);
            }

            Assert.AreEqual(power.ExtraData.Length, 5);
            for (int i = 0; i < power.ExtraData.Length; i++)
            {
                Assert.AreEqual(power.ExtraData[i], i);
            }
        }
    }
}