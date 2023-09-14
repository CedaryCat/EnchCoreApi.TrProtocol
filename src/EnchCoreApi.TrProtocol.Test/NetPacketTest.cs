using EnchCoreApi.TrProtocol.NetPackets;
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
    }
}