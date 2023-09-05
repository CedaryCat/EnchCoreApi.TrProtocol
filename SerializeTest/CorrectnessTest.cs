using EnchCoreApi.TrProtocol;
using EnchCoreApi.TrProtocol.NetPackets;
using System.Runtime.InteropServices;

namespace SerializeTest {
    public unsafe class CorrectnessTest {
        void* buffer;
        public unsafe CorrectnessTest() {
            buffer = (void*)Marshal.AllocHGlobal(1024);

            var hello = new ClientHello("Terraria");

            var b = new Span<byte>(buffer, 1024);
            int index = 0;
            hello.WriteContent(ref buffer);

            var arr = b[..index].ToArray();
            var str = string.Join(",", arr);
            Console.WriteLine(str);

            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);
            bw.Write("Terraria");
            Console.WriteLine(string.Join(",", ms.ToArray()));
        }

        public void Dispose() {
            Marshal.FreeHGlobal((IntPtr)buffer);
        }
    }
}
