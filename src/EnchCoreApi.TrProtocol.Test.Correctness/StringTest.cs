using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnchCoreApi.TrProtocol.Test.Correctness
{
    [TestClass]
    public class StringTest
    {
        public const string Text = "AAAAAAAAAAAAAAAA";
        [TestMethod]
        public unsafe void StringWrite()
        {
            byte[] buffer1 = new byte[1024];
            byte[] buffer2 = new byte[1024];

            using var bw = new BinaryWriter(new MemoryStream(buffer1));
            bw.Write(Text);

            long packetContentLen;
            fixed (void* ptr = buffer2)
            {
                var p = ptr;
                CommonCode.WriteString(ref p, Text);
                packetContentLen = (long)p - (long)ptr;
            }

            Assert.AreEqual(packetContentLen, bw.BaseStream.Position);

            for (int i = 0; i < bw.BaseStream.Position; i++)
            {
                Assert.AreEqual(buffer1[i], buffer2[i]);
            }
        }
        [TestMethod]
        public unsafe void StringRead()
        {
            byte[] buffer1 = new byte[1024];
            byte[] buffer2 = new byte[1024];

            using var bw = new BinaryWriter(new MemoryStream(buffer1));
            bw.Write(Text);

            long packetContentLen;
            fixed (void* ptr = buffer1)
            {
                var p = ptr;
                var text = CommonCode.ReadString(ref p);
                Assert.AreEqual(text, Text);
                packetContentLen = (long)p - (long)ptr;
            }

            Assert.AreEqual(packetContentLen, bw.BaseStream.Position);
        }
    }
}
