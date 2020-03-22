using NetMQ;
using NetMQ.Sockets;
using System;

namespace CompositeVideoOscilloscope {
    public class Output : IDisposable {
        public int NoOfBytes = 0;
        readonly PublisherSocket Publisher;

        public Output(string address) {
            Publisher = new PublisherSocket();
            Publisher.Bind(address);
        }

        public void Send(byte[] values, double sampleRate) {
            var msg = new NetMQMessage();
            msg.Append((long)sampleRate);
            msg.Append(values);
            Publisher.SendMultipartMessage(msg);
            NoOfBytes += values.Length;
        }

        public void Dispose() {
            Publisher.Dispose();
            NetMQConfig.Cleanup(block: false);
        }
    }
}
