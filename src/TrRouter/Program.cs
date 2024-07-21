using System.Net.Sockets;

namespace TrRouter
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var r = new TrRouter(7776, new System.Net.IPEndPoint(System.Net.IPAddress.Parse("127.0.0.1"), 7777));
            r.Start();
            while (true)
            {
                Thread.Sleep(1000);
            }
        }
    }
}
