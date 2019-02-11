using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CompositeVideoOscilloscope {
    public class Output {
        readonly int PacketSize = 256;
        readonly BlockingCollection<byte> Queue = new BlockingCollection<byte>();

        bool Connected = false;

        public void Add(double value) {
            if (Connected) {
                Queue.Add((byte)(255 * value));
            }
        }

        public async Task Run(CancellationToken canceller) {
            var listener = new TcpListener(IPAddress.Loopback, 8080);
            listener.Start();
            while (!canceller.IsCancellationRequested) {
                Connected = false;

                using (var client = await listener.AcceptTcpClientAsync()) {
                    while (Queue.TryTake(out byte _)) { };
                    Connected = true;

                    using (var stream = client.GetStream()) {
                        while (client.Connected && !canceller.IsCancellationRequested) {
                            var buffer = new byte[PacketSize];
                            for (int i = 0; i < PacketSize; i++) {
                                while (!Queue.TryTake(out buffer[i])) {
                                    await Task.Delay(1);
                                }
                            }
                            await stream.WriteAsync(buffer, 0, PacketSize);
                        }
                    }
                }

            }
        }
    }
}
