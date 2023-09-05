using EnchCoreApi.TrProtocol.NetPackets;
using System.Net.Sockets;
using System.Net;
using System;

namespace TrClient {
    internal class Program {
        static void Main(string[] args) {
            var client = new TrClient(false);
            var ip = "127.0.0.1";
            ushort port = 7777;
            /*
            ip = "43.248.184.35";
            port = 7777;*/
            var password = "";
            client.Username = "test";
            /*
            Console.Write("ip>");
            var ip = Console.ReadLine();
            Console.Write("port>");
            var port = ushort.Parse(Console.ReadLine());
            Console.Write("password>");
            var password = Console.ReadLine();
            Console.Write("username>");
            client.Username = Console.ReadLine();*/

            client.OnChat += (o, t, c) => Console.WriteLine(t);
            client.OnMessage += (o, t) => Console.WriteLine(t);
            bool shouldSpam = false;

            client.On<LoadPlayer>(_ =>
                    client.Send(new ClientUUID(Guid.Empty.ToString())));
            client.On<WorldData>(_ => {
                if (!shouldSpam) {
                    return;
                }
                for (; ; )
                {
                    client.Send(new RequestWorldInfo());
                    client.ChatText("/logout");
                }
            });

            new Thread(() => {
                for (; ; )
                {
                    var t = Console.ReadLine();
                    if (t == "/chatspam") shouldSpam = true;
                    else client.ChatText(t);
                }
            }).Start();

            client.GameLoop(new IPEndPoint(IPAddress.Parse(ip), port), password);
        }
    }
}