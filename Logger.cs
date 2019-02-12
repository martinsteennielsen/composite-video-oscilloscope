using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CompositeVideoOscilloscope {
    public class Logger {
        Output Output;

        public Logger(Output output) {
            Output = output;
        }

        public async Task Run(CancellationToken canceller) {
            while (!canceller.IsCancellationRequested) {
                Console.WriteLine($"Connected:{ Output.Connected}");
                await Task.Delay(100);
            }
        }
    }
}
