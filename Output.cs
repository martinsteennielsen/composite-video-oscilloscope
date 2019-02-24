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
            Publisher.Options.SendHighWatermark = 2;
            Publisher.Options.SendHighWatermark = 5;
            Publisher.Bind(address);
        }

        public void Set(List<double> values) {
            Publisher.SendFrame(values.Select(x => (byte)(x * 255.0)).ToArray());
        }

        public void Dispose() {
            Publisher.Dispose();
            NetMQConfig.Cleanup(block: false);
        }
    }
}
