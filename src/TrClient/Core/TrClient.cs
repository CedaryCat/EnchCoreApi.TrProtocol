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
using EnchCoreApi.TrProtocol.Models;
using System.Reflection;

namespace TrClient {
    public unsafe partial class TrClient : IDisposable {
        private TcpClient client;

        public byte PlayerSlot { get; private set; }
        public string CurRelease = "Terraria279";
        public string Username = "";
        public bool IsPlaying { get; private set; }

        private NetworkStream netStream;
        private readonly byte[] readBuffer;
        private readonly byte[] sendBuffer;
        public bool Debug;
        public string uuid;

        public TrClient(bool debug, string uuid = "", string serverPassword = "") {
            Debug = debug;
            this.uuid = uuid;
            password = serverPassword;
            readBuffer = new byte[1024 * 521];
            sendBuffer = new byte[1024 * 521];
            InternalOn();
        }
        public void Dispose() {
        }
        public void KillServer() {
            client.GetStream().Write(new byte[] { 0, 0 }, 0, 2);
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
            Send(new ClientUUID(uuid));
            Send(new PlayerHealth(0, 100, 100));
            for (short i = 0; i < 280; i++)
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

        private readonly Dictionary<Type, Action<NetPacket>> handlers = new();
        readonly HashSet<MessageID> registeredMessages = new();
        readonly HashSet<NetModuleType> registeredModules = new();
        public void On<T>(Action<T> handler) where T : NetPacket {
            void Handler(NetPacket p) => handler((T)p);

            if (handlers.TryGetValue(typeof(T), out var val))
                handlers[typeof(T)] = val + Handler;
            else handlers.Add(typeof(T), Handler);

            if (typeof(T).BaseType == typeof(NetModulesPacket)) {
                registeredMessages.Add(MessageID.NetModules);

                var ms = typeof(T).GetTypeInfo().DeclaredMethods;
                var method = ms.First(m => m.Name == "get_ModuleType");
                var ptr = method.MethodHandle.GetFunctionPointer();
                registeredModules.Add(((delegate*<T, NetModuleType>)ptr)(null!));
            }
            else {
                var ms = typeof(T).GetTypeInfo().DeclaredMethods;
                var method = ms.First(m => m.Name == "get_Type");
                var ptr = method.MethodHandle.GetFunctionPointer();
                registeredMessages.Add(((delegate*<T, MessageID>)ptr)(null!));
            }
        }

        private WorldData worldData = new WorldData(1000, 1000, 1000, 1000, "", Array.Empty<byte>());
        private void InternalOn() {

            On<RequestPassword>(_ => Send(new SendPassword(password)));
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
            On<WorldData>(wd => {
                worldData = wd;
                if (!IsPlaying) {
                    TileGetSection(100, 100);
                    IsPlaying = true;
                }
            });
            On<StartPlaying>(_ => {
                Spawn(worldData.SpawnX, worldData.SpawnY);
                IsSpawned = true;
            });
        }

        public string password;

        public bool connected;
        public bool SentHello { get; private set; } = false;
        public bool IsSpawned { get; private set; } = false;

        public PlayerControls? updateCtrl;
        public Vector2 updatePos;
        DateTime lastCtrlUpdate;

        public void ProcessPackets() {
            if (!connected) {
                return;
            }

            if (!SentHello) {
                Hello(CurRelease);
            }

            while (recievedPkg.TryDequeue(out var packet)) {
                try {
                    if (Debug) Console.WriteLine(packet.GetType().Name);

                    if (handlers.TryGetValue(packet.GetType(), out var handler)) handler(packet);

                    //else Console.WriteLine($"[Warning] not processed packet type {packet}");
                }
                catch {

                }
            }

            if (IsSpawned && (DateTime.Now - lastCtrlUpdate).TotalMilliseconds > 3000) {
                lastCtrlUpdate = DateTime.Now;
                PlayerControls ctrl;
                if (updateCtrl is null) {
                    var pos = updatePos;
                    if (pos == default) {
                        pos = new Vector2(worldData.SpawnX * 16, (worldData.SpawnY - 3) * 16);
                    }
                    ctrl = new PlayerControls(default, default, default, default, default, default, pos, default, default, default);
                }
                else {
                    ctrl = updateCtrl;
                }
                Send(ctrl);
            }
        }
    }
}
