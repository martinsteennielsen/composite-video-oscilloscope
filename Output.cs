using NetMQ;
using NetMQ.Sockets;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace CompositeVideoOscilloscope {
    public class Output {
        readonly Timing Timing;
        readonly int PacketSize;
        readonly ConcurrentQueue<byte> Queue = new ConcurrentQueue<byte>();

        public void Add(double value) {
            Queue.Enqueue((byte)(255 * value));
        }

        public Output(Timing timing) {
            Timing = timing;
            PacketSize = 200;
        }

        public async Task Run(CancellationToken canceller) {
            var buffer = new byte[PacketSize];

            using (var pub = new PublisherSocket()) {
                pub.Bind("tcp://127.0.0.1:10000");
                while (!canceller.IsCancellationRequested) {
                    while (Queue.Count < PacketSize) {
                        await Task.Delay(100);
                    }
                    for (int i = 0; i < PacketSize; i++) {
                        Queue.TryDequeue(out buffer[i]);
                    }
                    pub.SendFrame(buffer);
                }
            }
        }
    }
}
