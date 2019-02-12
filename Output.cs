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
        readonly BlockingCollection<byte> Queue = new BlockingCollection<byte>();

        public void Add(double value) {
            Queue.Add((byte)(255 * value));
        }

        public Output(Timing timing) {
            Timing = timing;
            PacketSize = 10*(int)(timing.LineTime / timing.DotTime);
        }

        public async Task Run(CancellationToken canceller) {
            var buffer = new byte[PacketSize];

            using (var pub = new PublisherSocket()) {
                pub.Bind("tcp://127.0.0.1:10000");
                while (!canceller.IsCancellationRequested) {
                    for (int i = 0; i < PacketSize; i++) {
                        while (!Queue.TryTake(out buffer[i])) {
                            await Task.Delay((int)(10*1000*Timing.LineTime));
                        }
                    }
                    pub.SendFrame(buffer);
                }
            }
        }
    }
}
