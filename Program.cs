using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CompositeVideoOscilloscope {
    class Program {

        static void Main(string[] args) {
            string settingsFile = args.Any() ? args[0] : "oscilloscope_controls.json";

            using (var output = new Output(address: "tcp://*:10001")) {
                var controller = new Controller(settingsFile);
                var oscilloscope = new Oscilloscope(output, controller);
                var canceller = new CancellationTokenSource();
                Task.Run(() => oscilloscope.Run(canceller.Token));

                var command = Console.ReadLine();
                while (command != "") {
                    controller.Commands.Enqueue(command);
                    command = Console.ReadLine();
                }

                canceller.Cancel();
            }
        }
    }
}