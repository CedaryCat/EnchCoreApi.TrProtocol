using System;
using System.Collections.Concurrent;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace TrClient.Core
{
    public class CachedSingleThreadSendSocket
    {
        readonly TcpClient _connection;
        public int SentCombinedPackets { get; private set; }
        readonly int id = instanceCount++;
        static int instanceCount = 0;

        readonly static ConcurrentCollection<CachedSingleThreadSendSocket> aliveSockets = new();
        public CachedSingleThreadSendSocket(TcpClient tcpClient) {
            _connection = tcpClient;
            aliveSockets.TryAdd(id, this);
        }
        static CachedSingleThreadSendSocket() => RunTasks();

        private readonly Channel<byte[]> _channel =
            Channel.CreateUnbounded<byte[]>();

        private static void RunTasks() {
            Task.Run(async () => {
                Thread.CurrentThread.Name = "ThreadSendSocket";
                while (true) {
                    foreach (var socket in aliveSockets) {
                        try {
                            if (socket.shouldRemove) {
                                aliveSockets.TryRemove(socket.id);
                            }
                            else if (Interlocked.CompareExchange(ref socket.completedProcessPacket, 0, 1) == 1) {
                                socket.ProcessPacket();
                            }
                        }
                        catch { }
                    }
                    await Task.Delay(10);
                }
            });
        }
        int cachedPos = 0;
        readonly byte[] cache = new byte[1024];
        volatile int completedProcessPacket = 1;
        bool shouldRemove = false;
        private async void ProcessPacket() {
            try {
                bool available = await _channel.Reader.WaitToReadAsync();
                NetworkStream? stream = null;
                if (_connection.Connected) {
                    if (!(stream = _connection.GetStream()).CanWrite) {
                        stream = null;
                    }
                }

                while (_channel.Reader.TryRead(out var buffer)) {
                    if (stream is not null) {
                        if (cache.Length - cachedPos >= buffer.Length) {
                            Buffer.BlockCopy(buffer, 0, cache, cachedPos, buffer.Length);
                            cachedPos += buffer.Length;
                        }
                        else {
                            SentCombinedPackets++;
                            await stream.WriteAsync(cache.AsMemory(0, cachedPos));
                            cachedPos = 0;
                            if (buffer.Length > cache.Length) {
                                SentCombinedPackets++;
                                await stream.WriteAsync(buffer.AsMemory(0, buffer.Length));
                            }
                            else {
                                Buffer.BlockCopy(buffer, 0, cache, 0, buffer.Length);
                                cachedPos = buffer.Length;
                            }
                        }
                    }
                }

                if (cachedPos > 0 && stream is not null) {
                    SentCombinedPackets++;
                    await stream.WriteAsync(cache.AsMemory(0, cachedPos));
                    cachedPos = 0;
                }

                if (!available || stream is null) {
                    shouldRemove = true;
                }
            }
            catch (Exception ex) {
                shouldRemove = true;
            }
            finally {
                Interlocked.Exchange(ref completedProcessPacket, 1);
            }
        }
        public unsafe void AsyncSend(byte[] data, int offset, int size) {
            if (!_connection.Connected || !_connection.GetStream().CanWrite) {
                _channel.Writer.TryComplete();
                return;
            }
            var buffer = new byte[size];
            Buffer.BlockCopy(data, offset, buffer, 0, size);
            _channel.Writer.TryWrite(buffer);
        }
        public void Close() {
            this._connection!.Close();
            _channel.Writer.TryComplete();
        }

        public async ValueTask<int> AsyncReceive(byte[] data, int offset, int size) {
            try {
                var read = await this._connection.GetStream().ReadAsync(data.AsMemory(offset, size));
                return read;
            }
            catch (Exception ex) {
                shouldRemove = true;
                return -1;
            }
        }
    }
}
