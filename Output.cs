using NetMQ;
using NetMQ.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CompositeVideoOscilloscope {
    public class Output : IDisposable {
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
        }

        public void Dispose() {
            Publisher.Dispose();
            NetMQConfig.Cleanup(block: false);
        }
    }
}
