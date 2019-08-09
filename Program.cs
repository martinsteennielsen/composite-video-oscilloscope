using System;
using System.Threading;
using System.Threading.Tasks;

namespace CompositeVideoOscilloscope {
    class Program {

        static void Main(string[] args) {
            using (var output = new Output(address: "tcp://*:10001")) {
                var oscilloscope = new Oscilloscope(output);
                var canceller = new CancellationTokenSource();
                Task.Run(() => oscilloscope.Run(canceller.Token));
                Console.WriteLine("Press enter to terminate");
                Console.ReadLine();
                canceller.Cancel();
            }
        }
    }
}