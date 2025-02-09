using System.Net.Sockets;
using EnchCoreApi.TrProtocol;
using EnchCoreApi.TrProtocol.Models.Interfaces;
using System.Runtime.CompilerServices;
using EnchCoreApi.TrProtocol.Models;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace TrClient
{
	unsafe partial class TrClient
	{
		public void Connect(string hostname, int port) {
			client = new TcpClient() { 
				NoDelay = true
			};
			client.Connect(hostname, port);
			netStream = client.GetStream();
			connected = true;

			NetThread();
		}
		protected ConcurrentQueue<NetPacket> recievedPkg = new ConcurrentQueue<NetPacket>();
		protected AutoResetEvent recievedSignal = new AutoResetEvent(false);
		void Receive_all() {
			var totalLen = netStream.Read(readBuffer, 0, 1024 * 32);
			fixed (void* ptr = readBuffer) {
				var readPtr = ptr;
				while (totalLen > 0) {
					var readPtrOld = readPtr;

					var packetLen = Unsafe.Read<short>(readPtr);
					readPtr = Unsafe.Add<byte>(readPtr, 2);
					var pkgType = (MessageID)Unsafe.Read<byte>(readPtr);

					if (Read(pkgType, ref readPtr, Unsafe.Add<byte>(readPtrOld, packetLen), out var pkg)) {
						recievedPkg.Enqueue(pkg);

						int sizeReaded = (int)((long)readPtr - (long)readPtrOld);

						if (sizeReaded != packetLen) {
							throw new Exception($"warning: packet '{pkg.GetType().Name}' len '{packetLen}' but readed '{sizeReaded}'");
						}
					}

					totalLen -= packetLen;
				}
			}
		}

		int lastStartPos = 0;
		void Receive_smart() {

			var totalLen = lastStartPos + netStream.Read(readBuffer, lastStartPos, readBuffer.Length - lastStartPos);
			lastStartPos = 0;

            fixed (void* ptr = readBuffer) {
				var readPtr = ptr;
				while (totalLen > 0) {
					var readPtrOld = readPtr;

					var packetLen = Unsafe.Read<short>(readPtr);

					if (totalLen < packetLen) {
                        for (int i = 0; i < totalLen; i++) {
							readBuffer[i] = Unsafe.Read<byte>(Unsafe.Add<byte>(readPtr, i));
                        }
						lastStartPos = totalLen;
                        break;
                    }

                    readPtr = Unsafe.Add<byte>(readPtr, 2);

					bool shouldRead = false;
					var pkgType = (MessageID)Unsafe.Read<byte>(readPtr);
					if (pkgType != MessageID.NetModules) {
						shouldRead = registeredMessages.Contains(pkgType);
					}
					else {
						shouldRead = registeredModules.Contains((NetModuleType)Unsafe.Read<short>(Unsafe.Add<byte>(readPtr, 1)));
					}

					if (shouldRead) {
						if (Read(pkgType, ref readPtr, Unsafe.Add<byte>(readPtrOld, packetLen), out var pkg)) {
							recievedPkg.Enqueue(pkg);

							int sizeReaded = (int)((long)readPtr - (long)readPtrOld);

							if (sizeReaded != packetLen) {
								throw new Exception($"warning: packet '{pkg.GetType().Name}' len '{packetLen}' but readed '{sizeReaded}'");
							}
						}
					}
					else {
						readPtr = Unsafe.Add<byte>(readPtrOld, packetLen);
					}
					totalLen -= packetLen;
				}
			}
		}
        static unsafe bool Read(MessageID type, ref void* readPtr, void* endPtr, [NotNullWhen(true)] out NetPacket? packet) {
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
		readonly ConcurrentQueue<NetPacket> sendQueue = new ConcurrentQueue<NetPacket>();
		readonly AutoResetEvent sendSignal = new AutoResetEvent(false);
		public void Send(NetPacket packet) {
			sendQueue.Enqueue(packet);
			sendSignal.Set();
		}
		void NetThread() {
			Task.Run(() => {
				while (connected && netStream.CanRead) {
					Receive_smart();
					if (!recievedPkg.IsEmpty) {
						recievedSignal.Set();
					}
				}
			});
			Task.Run(() => {
				while (connected && netStream.CanWrite) {
					while (sendQueue.TryDequeue(out var packet)) {
						try {
							if (packet is IPlayerSlot ips) ips.PlayerSlot = PlayerSlot;

							fixed (void* sendBuffer = this.sendBuffer) {
								var ptr = Unsafe.Add<byte>(sendBuffer, 2);
								packet.WriteContent(ref ptr);
								var size = (short)((long)ptr - (long)sendBuffer);
								Unsafe.Write(sendBuffer, size);

								var arr = new ReadOnlySpan<byte>(sendBuffer, size).ToArray();
								netStream.BeginWrite(arr, 0, arr.Length, null, null);

								if (Debug) {
									Console.WriteLine($"[{packet.GetType().Name}]{string.Join(",", arr.Select(b => $"{b:x2}"))}");
								}
							}
						}
						finally {

						}
					}
					sendSignal.WaitOne();
				}
			});
		}
	}
}
