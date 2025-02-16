using System.Net.Sockets;
using EnchCoreApi.TrProtocol;
using EnchCoreApi.TrProtocol.Models.Interfaces;
using System.Runtime.CompilerServices;
using EnchCoreApi.TrProtocol.Models;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using TrClient.Core;
using System.Threading.Channels;
using System.Runtime.InteropServices;

namespace TrClient
{
	partial class TrClient
    {
        private CachedSingleThreadSendSocket socket;
        static ConcurrentCollection<TrClient> runningClients = new ConcurrentCollection<TrClient>();
        readonly int id = instanceCount++;
        static int instanceCount = 0;
        public void Connect(string hostname, int port) {
            var client = new TcpClient() { 
				NoDelay = true
			};
			client.Connect(hostname, port);
            socket = new CachedSingleThreadSendSocket(client);
            connected = true;

            runningClients.TryAdd(id, this);
        }
        static TrClient() {
            RecieveThread();
        }
        static void RecieveThread() {
            Task.Run(async () => {
                Thread.CurrentThread.Name = "RecievePacketThread";
                while (true) {
                    foreach (var client in runningClients) {
                        try {
                            if (Interlocked.CompareExchange(ref client.completedReceive, 0, 1) == 1) {
                                client.Receive();
                            }
                        }
                        catch {
                            runningClients.TryRemove(client.id);
                        }
                    }
                    await Task.Delay(10);
                }
            });
        }

        volatile int completedReceive = 1;
        readonly Channel<NetPacket> recievedPkts = Channel.CreateUnbounded<NetPacket>();

        async void Receive() {

            var readed = await socket.AsyncReceive(readBuffer, lastStartPos, readBuffer.Length - lastStartPos);
            if (readed >= 0) {
                var totalLen = lastStartPos + readed;
                lastStartPos = 0;
                ProcessRecievedBytes(totalLen);
            }
            else {
                runningClients.TryRemove(id);
            }

            Interlocked.Exchange(ref completedReceive, 1);
        }

        int lastStartPos = 0;
		unsafe void ProcessRecievedBytes(int totalLen) {

            fixed (void* ptr = readBuffer) {
                var currentReadPtr = ptr;
                while (totalLen > 0) {
                    var beginReadPtr = currentReadPtr;

                    var packetLen = Unsafe.Read<short>(currentReadPtr);

                    if (totalLen < packetLen) {
                        for (int i = 0; i < totalLen; i++) {
                            readBuffer[i] = Unsafe.Read<byte>(Unsafe.Add<byte>(currentReadPtr, i));
                        }
                        lastStartPos = totalLen;
                        break;
                    }

                    currentReadPtr = Unsafe.Add<byte>(currentReadPtr, 2);

                    bool shouldRead = false;
                    var pkgType = (MessageID)Unsafe.Read<byte>(currentReadPtr);
                    if (pkgType != MessageID.NetModules) {
                        shouldRead = registeredMessages.Contains(pkgType);
                    }
                    else {
                        shouldRead = registeredModules.Contains((NetModuleType)Unsafe.Read<short>(Unsafe.Add<byte>(currentReadPtr, 1)));
                    }

                    if (shouldRead) {
                        if (ReadPacket(pkgType, ref currentReadPtr, Unsafe.Add<byte>(beginReadPtr, packetLen), out var pkt)) {
                            recievedPkts.Writer.TryWrite(pkt);

                            int sizeReaded = (int)((long)currentReadPtr - (long)beginReadPtr);

                            // Console.WriteLine($"[{pkt.Type}]Len:{packetLen}, Read:{sizeReaded}");

                            if (sizeReaded != packetLen) {
                                throw new Exception($"warning: packet '{pkt.GetType().Name}' len '{packetLen}' but readed '{sizeReaded}'");
                            }
                        }
                    }
                    else {
                        currentReadPtr = Unsafe.Add<byte>(beginReadPtr, packetLen);
                    }
                    totalLen -= packetLen;
                }
            }
        }
        static unsafe bool ReadPacket(MessageID type, ref void* readPtr, void* endPtr, [NotNullWhen(true)] out NetPacket? packet) {
			try {
				packet = NetPacket.ReadNetPacket(ref readPtr, endPtr, false);
				return true;
			}
			catch (Exception e) {
				Console.ForegroundColor = ConsoleColor.Red;
				var msg = $"Exception caught when trying to parse packet [{type}]\n{e}";
				Console.WriteLine(msg);
				File.AppendAllText("log.txt", msg + "\n");
				Console.ResetColor();

				packet = null;
				return false;
			}
		}
        public unsafe void Send(NetPacket packet) {
            try {
                if (packet is IPlayerSlot ips) ips.PlayerSlot = PlayerSlot;

                var ptr_begin = (void*)Marshal.AllocHGlobal(1024 * 16);

                var ptr = Unsafe.Add<byte>(ptr_begin, 2);
                packet.WriteContent(ref ptr);
                var size = (short)((long)ptr - (long)ptr_begin);
                Unsafe.Write(ptr_begin, size);

                var arr = new ReadOnlySpan<byte>(ptr_begin, size).ToArray();

                socket.AsyncSend(arr, 0, size);

                Marshal.FreeHGlobal((nint)ptr_begin);

                if (Debug) {
                    Console.WriteLine($"{ToString()}[↑][{packet.GetType().Name}]{string.Join(",", arr.Select(b => $"{b:x2}"))}");
                }
            }
            finally {

            }
        }
	}
}
