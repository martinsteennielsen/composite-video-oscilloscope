using NetMQ;
using NetMQ.Sockets;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace CompositeVideoOscilloscope {
    public class Output : IDisposable {
        readonly PublisherSocket Publisher;

        public Output(TimingConstants timing) {
            Publisher = new PublisherSocket();
            Publisher.Bind("tcp://*:10001");
        }

        public void Set(List<double> values) {
            Publisher.SendFrame(values.Select(x=>(byte)(x*255.0)).ToArray());
        }

        public void Dispose() {
            Publisher.Dispose();
        }
    }
}
