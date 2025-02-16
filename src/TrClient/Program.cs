using EnchCoreApi.TrProtocol.NetPackets;
using System.Net.Sockets;
using System.Net;
using System;
using Microsoft.Xna.Framework;
using System.Xml.Linq;
using EnchCoreApi.TrProtocol;

namespace TrClient {
    internal class Program
    {
        static string?[] names = new string?[byte.MaxValue + 1];
        static WorldData worldData = new WorldData(1000, 1000, 1000, 1000, "aaa", Array.Empty<byte>());
        static void Main(string[] args) {
            var param = new Parameters(args);

            int numInGroup = param.NumInGroup;
            int groupIndex = param.GroupIndex;
            string ip = param.IP;
            ushort port = param.Port;
            string target = param.SimulateTarget;

            TrClient[] clients = new TrClient[numInGroup];
            DateTime[] schedule = new DateTime[numInGroup];

            for (int i = 0; i < numInGroup; i++) {
                clients[i] = SetupClient(groupIndex * numInGroup + i, target);
                schedule[i] = DateTime.Now + TimeSpan.FromSeconds(0.3333 * i);
            }

            while (true) {
                for (int i = 0; i < numInGroup; i++) {
                    var client = clients[i];

                    if (!client.connected && schedule[i] < DateTime.Now) {
                        client.Connect(ip, port);
                    }

                    try {
                        client.ProcessClientLogic();

                        //if (client.IsSpawned && rand.Next(30) == 0) {
                        //    var pos = new Vector2(worldData.SpawnX * 16 + 8, worldData.SpawnY * 16 - 48);
                        //    if (client.updatePos != default) {
                        //        pos = client.updatePos + new Vector2(8, 0); ;
                        //    }
                        //    if (client.updateCtrl != default) {
                        //        pos = client.updateCtrl.Position + new Vector2(8, 0);
                        //    }
                        //    client.Send(new SyncProjectile(
                        //        (short)rand.Next(1000),
                        //        pos,
                        //        new Vector2(rand.Next(-5, 5), -5),
                        //        0,
                        //        931,
                        //        new Terraria.BitsByte(false, false, false, false, true, true, false, false),
                        //        default,
                        //        default,
                        //        default,
                        //        default,
                        //        50,
                        //        1,
                        //        0,
                        //        0,
                        //        0));
                        //}
                    }
                    catch { 
                        client.connected = false;
                    }
                }
                Thread.Sleep(15);
            }
        }
        static TrClient SetupClient(int i, string target) {
            var client = new TrClient(false, Guid.Empty.ToString());
            var rand = new Random(i * DateTime.Now.Ticks.GetHashCode());
            client.Username = "test" + i;

            client.OnChat += (o, t, c) => Console.WriteLine(t);
            client.OnMessage += (o, t) => Console.WriteLine(t);

            client.On<LoadPlayer>(pkg => {
                names[pkg.PlayerSlot] = client.Username;
            });
            client.On<WorldData>(pkg => {
                worldData = pkg;
            });
            client.On<ResetItemOwner>(pkg => {
                client.Send(new ItemOwner(pkg.ItemSlot, 255));
            });
            client.On<SyncPlayer>(pkg => {
                names[pkg.PlayerSlot] = pkg.Name;
                if (names[pkg.PlayerSlot] == target) {
                    pkg.Name = client.Username;
                    client.Send(pkg);
                }
            });
            client.On<PlayerBuffs>(pkg => {
                if (names[pkg.PlayerSlot] == target) {
                    client.Send(pkg);
                }
            });
            client.On<SyncEquipment>(pkg => {
                if (names[pkg.PlayerSlot] == target) {
                    client.Send(pkg);
                }
            });
            client.On<PlayerControls>(pkg => {
                if (names[pkg.PlayerSlot] == target) {
                    client.updateCtrl = pkg;

                    var x = i / 6;
                    var y = i % 6;
                    client.updateCtrl.Position -= new Vector2(32 * x, 48 * y);
                    client.Send(pkg);
                }
            });
            client.On<ItemAnimation>(pkg => {
                if (names[pkg.PlayerSlot] == target) {
                    client.Send(pkg);
                }
            });
            client.On<SyncProjectile>(pkg => {
                if (names[pkg.PlayerSlot] == target) {

                    var x = i / 6;
                    var y = i % 6;
                    pkg.Position -= new Vector2(32 * x, 48 * y);
                    client.Send(pkg);
                }
            });
            client.On<StrikeNPC>(pkg => {
                if (pkg.Knockback != 0.123456f) {
                    pkg.Knockback = 0.123456f;
                    client.Send(pkg);
                }
            });

            return client;
        }
    }
}