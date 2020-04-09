using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CompositeVideoOscilloscope {
    class Program {

        static void Main(string[] args) {
            string command = null;
            string settingsFile = args.Any() ? args[0] : "controls.json";

            void handleControls(Controls controls) {
                Console.Write($"\r {controls.BytesPrSecond / 1e6:F3} Mb/s, command --> ");

                if (command == "s") {
                    controls.Plot1.SubSamplePlot = !controls.Plot1.SubSamplePlot;
                    controls.Plot2.SubSamplePlot = !controls.Plot2.SubSamplePlot;
                } else if (command == "f") {
                    controls.RunMovements = !controls.RunMovements;
                } else if (command == "o") {
                    controls.EnableOutput = !controls.EnableOutput;
                } else if (command == "v") {
                    controls.VideoStandard =
                    (controls.VideoStandard == VideoStandard_.Pal10MhzProgessiv
                    ? VideoStandard_.Pal5MhzProgessiv : VideoStandard_.Pal10MhzProgessiv);
                } else if (command == "i") {
                    controls.VideoStandard =
                    (controls.VideoStandard == VideoStandard_.Pal5MhzProgessiv
                    ? VideoStandard_.Pal5MhzInterlaced : VideoStandard_.Pal5MhzProgessiv);
                } else if (command == "save") {
                    File.WriteAllText(settingsFile, JsonConvert.SerializeObject(controls, Formatting.Indented));
                } 
            command = null;
            }

            Controls readControls() {
                try {
                    return JsonConvert.DeserializeObject<Controls>(File.ReadAllText(settingsFile));
                } catch {
                    return null;
                }
            }

            using (var output = new Output(address: "tcp://*:10001")) {
                var controller = new Controller(handleControls, controls: readControls());
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