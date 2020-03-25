using System;
using System.Threading;
using System.Threading.Tasks;

namespace CompositeVideoOscilloscope {
    class Program {

        static void Main(string[] args) {
            string command = null;

            void handleControls(Controls controls) {
                Console.Write($"\r {controls.BytesPrSecond / 1e6:F3} Mb/s, command --> ");

                if (command == "s") {
                    controls.PlotControls.SubSamplePlot = !controls.PlotControls.SubSamplePlot;
                } else if (command == "f") {
                    controls.RunMovements = !controls.RunMovements;
                } else if (command == "o") {
                    controls.EnableOutput = !controls.EnableOutput;
                } else if (command == "v") {
                    controls.VideoStandard =
                    (controls.VideoStandard.Equals(VideoStandard.Pal5MhzProgessiv)
                    ? VideoStandard.Pal10MhzProgessiv : VideoStandard.Pal5MhzProgessiv);
                }
                command = null;
            }

            using (var output = new Output(address: "tcp://*:10001")) {
                var controller = new Controller(handleControls);
                var oscilloscope = new Oscilloscope(output, controller);
                var canceller = new CancellationTokenSource();
                Task.Run(() => oscilloscope.Run(canceller.Token));
                while (command != "") {
                    command = Console.ReadLine();
                }
                canceller.Cancel();
            }
        }
    }
}