using EnchCoreApi.TrProtocol;
using EnchCoreApi.TrProtocol.Interfaces;
using EnchCoreApi.TrProtocol.NetPackets;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace TrRouter
{
    public class TrRouter
    {
        List<ClientLoop> clients = new List<ClientLoop>();
        TcpListener listener;
        IPEndPoint Destination;

        void ListenLoop()
        {
            while (true)
            {
                clients.RemoveAll(c => {
                    if (c.Started && !c.Connected)
                    {
                        c.Close();
                        return true;
                    }
                    return false;
                });
                clients.Add(new ClientLoop(clients.Count, listener.AcceptSocket(), Destination));
            }
        }
        public TrRouter(int listenPort, IPEndPoint destination)
        {
            listener = new TcpListener(IPAddress.Any, listenPort);
            Destination = destination;
        }

        public void Start()
        {
            listener.Start();
            new Task(ListenLoop).Start();
        }
        public void Close()
        {
            listener.Stop();
        }

        class ClientLoop
        {
            public override string ToString() => Name is null ? Index.ToString() : Index + ":" + Name;
            public string? Name { get; private set; }
            public readonly int Index;
            public readonly bool Started = false;
            Socket remoteClientSocket;
            TcpClient ConnectToRemoteServer;

            NetworkStream serverStream;


            byte[] ReadBufferForRemoteServer = new byte[1024 * 1024];
            byte[] SendBufferForRemoteServer = new byte[1024 * 1024];

            byte[] ReadBufferForRemoteClient = new byte[1024 * 1024];
            byte[] SendBufferForRemoteClient = new byte[1024 * 1024];

            public bool Connected => remoteClientSocket.Connected && ConnectToRemoteServer.Connected;
            public ClientLoop(int index, Socket socket, IPEndPoint server) {
                Index = index;
                remoteClientSocket = socket;
                ConnectToRemoteServer = new TcpClient();
                try {
                    ConnectToRemoteServer.Connect(server);
                    serverStream = ConnectToRemoteServer.GetStream();
                    new Task(RemoteServerLoop).Start();
                    new Task(RemoteClientLoop).Start();
                }
                catch {

                }
                finally {
                    Started = true;
                }
            }
            unsafe void RemoteServerLoop()
            {
                while (ConnectToRemoteServer.Connected)
                {
                    try
                    {
                        int count = 0;
                        foreach (var (packet, packetHeader) in Receive(ReadBufferForRemoteServer, 0, serverStream.Read(ReadBufferForRemoteServer, 0, 1024 * 32), false))
                        {
                            if (packet is ISideDependent sideDependent)
                            {
                                sideDependent.IsServerSide = true; //
                            }
                            fixed (void* ptr = SendBufferForRemoteServer)
                            {
                                //skip the packet header
                                var ptr_current = Unsafe.Add<short>(ptr, 1);
                                //write packet
                                packet.WriteContent(ref ptr_current);
                                //get the packet header
                                var size_short = (short)((long)ptr_current - (long)ptr);
                                //write packet header value
                                Unsafe.Write(ptr, size_short);

                                count += size_short;
                                remoteClientSocket.Send(SendBufferForRemoteServer, 0, size_short, SocketFlags.None);
                                Console.WriteLine($"[{this}]Server:{packet.GetType().Name} >> Recieve:{packetHeader} & Send:{size_short} | EndPosition:{count}");

                                //terraria is based on the .netframework, and the compression algorithm is slightly different from net.
                                if (packetHeader != size_short && packet is not TileSection) {
                                    throw new Exception($"[{this}]Warning: send len '{size_short}' is not equal to recieve len '{packetHeader}'.");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[{this}]ServerEx:");
                        Console.WriteLine(ex);
                        ConnectToRemoteServer.Close();
                    }
                }
            }
            unsafe void RemoteClientLoop()
            {
                while (remoteClientSocket.Connected)
                {
                    var task = remoteClientSocket.ReceiveAsync(ReadBufferForRemoteClient, SocketFlags.None);
                    task.Wait();
                    try
                    {
                        int count = 0;
                        foreach (var (packet, packetHeader) in Receive(ReadBufferForRemoteClient, 0, task.Result, true))
                        {
                            if (packet is ISideDependent sideDependent)
                            {
                                sideDependent.IsServerSide = false;
                            }
                            if (packet is SyncPlayer syncPlayer) {
                                Name = syncPlayer.Name;
                            }
                            fixed (void* ptr = SendBufferForRemoteClient)
                            {
                                //skip the packet header
                                var ptr_current = Unsafe.Add<short>(ptr, 1);
                                //write packet
                                packet.WriteContent(ref ptr_current);
                                //get the packet header
                                var size_short = (short)((long)ptr_current - (long)ptr);
                                //write packet header value
                                Unsafe.Write(ptr, size_short);

                                count += size_short;

                                serverStream.Write(SendBufferForRemoteClient, 0, size_short);

                                Console.WriteLine($"[{this}]Client:{packet.GetType().Name} >> Recieve:{packetHeader} & Send:{size_short} | EndPosition:{count}");

                                //terraria is based on the .netframework, and the compression algorithm is slightly different from net.
                                if (packetHeader != size_short && packet is not TileSection) {
                                    throw new Exception($"[{this}]Warning: send len '{size_short}' is not equal to recieve len '{packetHeader}'.");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[{this}]ClientEx:");
                        Console.WriteLine(ex);
                        remoteClientSocket.Close();
                    }
                }
            }
            public static unsafe IEnumerable<(NetPacket,int)> Receive(byte[] readBuffer, int index, int count, bool isServerSize)
            {
                var array = ArrayPool<(NetPacket, int)>.Shared.Rent(1024);
                int arrayIndex = 0;

                var totalLen = count;
                fixed (void* ptr = readBuffer)
                {
                    var readPtr = Unsafe.Add<byte>(ptr, index);
                    while (totalLen > 0)
                    {
                        var readPtrOld = readPtr;

                        var packetHeader = Unsafe.Read<short>(readPtr);
                        readPtr = Unsafe.Add<short>(readPtr, 1);

                        var packet = NetPacket.ReadNetPacket(ref readPtr, Unsafe.Add<byte>(readPtrOld, packetHeader), isServerSize);
                        int sizeReaded = (int)((long)readPtr - (long)readPtrOld);
                        array[arrayIndex] = (packet, packetHeader);

                        if (sizeReaded != packetHeader)
                        {
                            throw new Exception($"warning: packet '{array[arrayIndex].Item1.GetType().Name}' len '{packetHeader}' but readed '{sizeReaded}'");
                        }

                        ++arrayIndex;
                        totalLen -= packetHeader;
                    }
                    if (totalLen < 0)
                    {
                        throw new Exception($"warning: packet extra read '{-totalLen}'");
                    }
                }
                var res = new (NetPacket, int)[arrayIndex];
                Array.Copy(array, 0, res, 0, arrayIndex);
                ArrayPool<(NetPacket, int)>.Shared.Return(array);
                return res;
            }
            public void Close()
            {
                remoteClientSocket?.Close();
                ConnectToRemoteServer?.Close();
                serverStream?.Close();
            }
        }
    }
}
