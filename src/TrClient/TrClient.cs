using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using EnchCoreApi.TrProtocol;
using EnchCoreApi.TrProtocol.NetPackets;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Terraria;
using EnchCoreApi.TrProtocol.Models.Interfaces;
using EnchCoreApi.TrProtocol.NetPackets.Modules;
using Terraria.Localization;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Buffers;
using System.Collections;
using System.Reflection.Metadata;

namespace TrClient {
    public unsafe class TrClient : IDisposable {
        private TcpClient client;

        public byte PlayerSlot { get; private set; }
        public string CurRelease = "Terraria279";
        public string Username = "";
        public bool IsPlaying { get; private set; }

        private NetworkStream netStream;
        private readonly byte[] readBuffer;
        private byte* sendBuffer;
        public bool Debug;

        public TrClient(bool debug) {
            Debug = debug;
            readBuffer = new byte[1024 * 32];
            sendBuffer = (byte*)Marshal.AllocHGlobal(1024 * 32);
            InternalOn();
        }
        public void Dispose() {
            Marshal.FreeHGlobal((IntPtr)sendBuffer);
        }

        public void Connect(string hostname, int port) {
            client = new TcpClient();
            client.Connect(hostname, port);
            netStream = client.GetStream();
        }

        public void Connect(IPEndPoint server, IPEndPoint? proxy = null) {
            if (proxy == null) {
                client = new TcpClient();
                client.Connect(server);
                netStream = client.GetStream();
                return;
            }

            client.Connect(proxy);

            //Console.WriteLine("Proxy connected to " + proxy.ToString());
            var encoding = new UTF8Encoding(false, true);
            using var sw = new StreamWriter(client.GetStream(), encoding, 4096, true) { NewLine = "\r\n" };
            using var sr = new StreamReader(client.GetStream(), encoding, false, 4096, true);
            sw.WriteLine($"CONNECT {server} HTTP/1.1");
            sw.WriteLine("User-Agent: Java/1.8.0_192");
            sw.WriteLine($"Host: {server}");
            sw.WriteLine("Accept: text/html, image/gif, image/jpeg, *; q=.2, */*; q=.2");
            sw.WriteLine("Proxy-Connection: keep-alive");
            sw.WriteLine();
            sw.Flush();

            var resp = sr.ReadLine();
            Console.WriteLine("Proxy connection; " + resp);
            if (!resp.StartsWith("HTTP/1.1 200")) throw new Exception();

            while (true) {
                resp = sr.ReadLine();
                if (string.IsNullOrEmpty(resp)) break;
            }
        }

        public void KillServer() {
            client.GetStream().Write(new byte[] { 0, 0 }, 0, 2);
        }
        public IEnumerable<NetPacket> Receive() {
            var array = ArrayPool<NetPacket>.Shared.Rent(1024);
            int arrayIndex = 0;

            var totalLen = netStream.Read(readBuffer, 0, 1024 * 32);
            fixed(void* ptr = readBuffer) {
                var readPtr = ptr;
                while (totalLen > 0) {
                    var readPtrOld = readPtr;

                    var packetLen = Unsafe.Read<short>(readPtr);
                    readPtr = Unsafe.Add<byte>(readPtr, 2);

                    array[arrayIndex] = NetPacket.ReadNetPacket(ref readPtr, packetLen - 2, false);
                    int sizeReaded = (int)((long)readPtr - (long)readPtrOld);

                    if (sizeReaded != packetLen) {

                        throw new Exception($"warning: packet '{array[arrayIndex].GetType().Name}' len '{packetLen}' but readed '{sizeReaded}'");
                    }

                    ++arrayIndex;
                    totalLen -= packetLen;
                }
            }
            var res = new NetPacket[arrayIndex];
            Array.Copy(array, 0, res, 0, arrayIndex);
            ArrayPool<NetPacket>.Shared.Return(array);
            return res;
        }
        public void Send(NetPacket packet) {
            if (packet is IPlayerSlot ips) ips.PlayerSlot = PlayerSlot;
            var ptr = Unsafe.Add<byte>(sendBuffer, 2);
            packet.WriteContent(ref ptr);
            var size = (short)((long)ptr - (long)sendBuffer);
            Unsafe.Write(sendBuffer, size);

            var arr = new Span<byte>(sendBuffer, size).ToArray();
            netStream.Write(arr);

            if (Debug) {
                Console.WriteLine($"[{packet.GetType().Name}]{string.Join(",", arr.Select(b => $"{b:x2}"))}");
            }
        }
        public void Hello(string message) {
            Send(new ClientHello(message));
        }

        public void TileGetSection(int x, int y) {
            Send(new RequestTileData(new Point(x, y)));
        }

        public void Spawn(short x, short y) {
            Send(new SpawnPlayer(0, new Point16(x, y), 0, 0, 0, PlayerSpawnContext.SpawningIntoWorld));
        }

        public void SendPlayer() {
            Send(new SyncPlayer(0, 0, 0, Username, default, default, default, default, default, default, default, default, default, default, default, default, default, default));
            Send(new PlayerHealth(0, 100, 100));
            for (byte i = 0; i < 73; ++i)
                Send(new SyncEquipment(0, i, 0, 0, 0));
        }

        public void ChatText(string message) {
            Send(new NetTextModule(new TextC2S() {
                Command = "Say",
                Text = message
            }, null, false));
        }

        public event Action<TrClient, NetworkTextModel, Color> OnChat;
        public event Action<TrClient, string> OnMessage;
        public Func<bool> shouldExit = () => false;

        //private Handlers handlers = new Handlers();

        //public void On<TPacket>(Action<TPacket> handler) where TPacket : NetPacket {
        //    handlers.Register(handler);
        //}
        //private class Handlers {
        //    protected readonly List<Delegate?> handlers = new (180);
        //    protected static int Count;
        //    public void Register<TPacket>(Action<TPacket> handler) where TPacket : NetPacket {

        //        var id = HandlersInternal<TPacket>.ID;

        //        while (handlers.Count <= id) {
        //            handlers.Add(null);
        //        }

        //        var deles = handlers[id];
        //        if (deles is not null) {
        //            handlers[id] = (Action<TPacket>)deles + handler;
        //        }
        //        else {
        //            handlers[id] = handler;
        //        }
        //    }
        //    public void Invoke<TPacket>(NetPacket packet) {

        //    }
        //    private sealed class HandlersInternal<TPacket> : Handlers where TPacket : NetPacket {
        //        public static readonly int ID;
        //        static HandlersInternal() {
        //            ID = Count++;
        //        }
        //    }
        //}
        private readonly Dictionary<Type, Action<NetPacket>> handlers = new();

        public void On<T>(Action<T> handler) where T : NetPacket {
            void Handler(NetPacket p) => handler(p as T);

            if (handlers.TryGetValue(typeof(T), out var val))
                handlers[typeof(T)] = val + Handler;
            else handlers.Add(typeof(T), Handler);
        }


        private void InternalOn() {

            //On<StatusText>(status => OnChat?.Invoke(this, status.Text, Color.White));
            On<NetTextModule>(text => OnChat?.Invoke(this, text.TextS2C?.Text ?? NetworkTextModel.Empty, text.TextS2C?.Color ?? Color.White));
            On<SmartTextMessage>(text => OnChat?.Invoke(this, text.Text, text.Color));
            On<Kick>(kick => {
                OnMessage?.Invoke(this, "Kicked : " + kick.Reason);
                connected = false;
            });
            On<LoadPlayer>(player => {
                PlayerSlot = player.PlayerSlot;
                SendPlayer();
                Send(new RequestWorldInfo());
            });
            On<WorldData>(_ => {
                if (!IsPlaying) {
                    TileGetSection(100, 100);
                    IsPlaying = true;
                }
            });
            On<StartPlaying>(_ => {
                Spawn(100, 100);

            });
        }

        public bool connected = false;

        public void GameLoop(string host, int port, string password) {
            Connect(host, port);
            GameLoopInternal(password);
        }
        public void GameLoop(IPEndPoint endPoint, string password, IPEndPoint proxy = null) {
            Connect(endPoint, proxy);
            GameLoopInternal(password);
        }
        private void GameLoopInternal(string password) {

            Console.WriteLine("Sending Client Hello...");
            Hello(CurRelease);

            /*TcpClient verify = new TcpClient();
            byte[] raw = Encoding.ASCII.GetBytes("-1551487326");
            verify.Connect(new IPEndPoint(endPoint.Address, 7980));
            verify.GetStream().Write(raw, 0, raw.Length);
            verify.Close();*/

            On<RequestPassword>(_ => Send(new SendPassword(password)));

            connected = true;
            while (connected && !shouldExit()) {
                foreach(var packet in Receive()) {
                    try {
                        if (Debug) {
                            Console.WriteLine(packet.GetType().Name);
                        }
                        if (handlers.TryGetValue(packet.GetType(), out var act))
                            act(packet);
                        else;
                        //Console.WriteLine($"[Warning] not processed packet type {packet}");
                    }
                    catch (Exception e) {
                        Console.ForegroundColor = ConsoleColor.Red;
                        var msg = $"Exception caught when trying to parse packet {packet.Type}\n{e}";
                        Console.WriteLine(msg);
                        File.AppendAllText("log.txt", msg + "\n");
                        Console.ResetColor();
                    }
                }
                Send(new PlayerControls(default, default, default, default, default, default, new Vector2(1000,1000), default, default, default));
            }

            client.Close();

        }
    }
}
