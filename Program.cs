using System;
using System.Threading;
using System.Threading.Tasks;

namespace CompositeVideoOscilloscope {
    class Program {

        static void Main(string[] args) {
            var output = new Output();
            var logger = new Logger(output);
            var oscilloscope = new Oscilloscope(output);

            var canceller = new CancellationTokenSource();

            Task.Run(() => output.Run(canceller.Token));
            Task.Run(() => logger.Run(canceller.Token));
            Task.Run(() => oscilloscope.Run(canceller.Token));
            Console.ReadLine();
            canceller.Cancel();
        }
    }
}