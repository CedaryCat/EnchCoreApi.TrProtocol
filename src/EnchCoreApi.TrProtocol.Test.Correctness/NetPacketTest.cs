using EnchCoreApi.TrProtocol.Models;
using EnchCoreApi.TrProtocol.NetPackets;
using EnchCoreApi.TrProtocol.NetPackets.Modules;
using EnchCoreApi.TrProtocol.Test.Correctness;
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

            long packetContentLen;

            byte[] buffer2 = new byte[1024];
            fixed (void* ptr = buffer2)
            {
                var p = ptr;
                hello.WriteContent(ref p);
                packetContentLen = (long)p - (long)ptr;
            }

            Assert.AreEqual(packetContentLen, bw.BaseStream.Position);

            for (int i = 0; i < bw.BaseStream.Position; i++)
            {
                Assert.AreEqual(buffer1[i], buffer2[i]);
            }
        }


        readonly byte[] NetCreativePowersModuleData = new byte[] { 
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
                power = (NetCreativePowersModule)NetPacket.ReadNetPacket(ref p, Unsafe.Add<byte>(ptr, 1 + 2 + 2 + 5), true);
            }

            Assert.AreEqual(power.ExtraData.Length, 5);
            for (int i = 0; i < power.ExtraData.Length; i++)
            {
                Assert.AreEqual(power.ExtraData[i], i);
            }
        }

        [TestMethod]
        public unsafe void TileSection()
        {
            var data = PacketData.tilesection1_3800_450_200_150;
            fixed(void* ptr = data)
            {
                var p = Unsafe.Add<short>(ptr, 1);
                var packet = NetPacket.ReadNetPacket(ref p, Unsafe.Add<byte>(ptr, data.Length), false);
            }
        }
    }
}