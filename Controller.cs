using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Linq.Expressions;


namespace CompositeVideoOscilloscope {

    public class Controller {
        public readonly Controls Controls;
        public readonly Queue<string> Commands = new Queue<string>();

        private readonly string ControlsFile; 
        private readonly Stopwatch Stopwatch = new Stopwatch();
        private readonly Movements Movements;

        public Controller(string controlsFile = null) {
            ControlsFile = controlsFile;

            Controls = LoadControls() ?? new Controls() {
                VideoStandard = VideoStandard.Pal5MhzProgessiv,
                Plot1 = new PlotControls {
                    SubSamplePlot = false,
                    NumberOfDivisions = 10,
                    Units = (.005, 0.5),
                    Trigger = new TriggerControls { Voltage = 0.6, Edge = 0 },
                    Location = new LocationControls { Left = 0.1, Top = 0.1, Right = 0.5, Bottom = 0.5, Angle = 0 }
                },
                Plot2 = new PlotControls {
                    SubSamplePlot = false,
                    NumberOfDivisions = 10,
                    Units = (.005, 0.5),
                    Trigger = new TriggerControls { Voltage = 0.6, Edge = 0 },
                    Location = new LocationControls { Left = 0.6, Top = 0.6, Right = 1.0, Bottom = 1.0, Angle = 0 }
                },
                RunMovements = true,
                EnableOutput = true,
            };
            Movements = new Movements(Controls);

            Movements.Add(0, -0.04, 0, member: x => x.Plot2.Location.Left);
            Movements.Add(0, -0.04, 0, member: x => Controls.Plot2.Location.Top);
            Movements.Add(Math.PI / 2, 0.4, 0.01, member: x => Controls.Plot2.Location.Angle);
            Movements.Add(-Math.PI / 2, -0.2, -0.01, member: x => Controls.Plot1.Location.Angle);
            Stopwatch.Start();
        }

        Controls LoadControls() {
            try {
                return JsonConvert.DeserializeObject<Controls>(File.ReadAllText(ControlsFile));
            } catch {
                return null;
            }
        }

        public async Task<Controls> Run(int noOfGeneratedBytes) {
            await Task.Yield();
            var currentTime = Stopwatch.Elapsed.TotalSeconds;
            // var elapsedTime = Controls.VideoStandard.Timing.FrameTime; //currentTime - Controls.CurrentTime;
            var elapsedTime = currentTime - Controls.CurrentTime;
            Controls.TimeMsCount += (int)(1000 * elapsedTime);
            if (Controls.TimeMsCount > 200) {
                Controls.BytesPrSecond = 1000.0 * Controls.ByteCount / Controls.TimeMsCount;
                Controls.ByteCount = noOfGeneratedBytes;
                Controls.TimeMsCount %= 200;
            } else {
                Controls.ByteCount += noOfGeneratedBytes;
            }

            Controls.CurrentTime = currentTime;
            if (Controls.RunMovements) {
                Movements.Run(Controls, elapsedTime);
            }
            
            if (Commands.Any()) {
                HandleCommand(Commands.Dequeue());
            }

            Console.Write($"\r {Controls.BytesPrSecond / 1e6:F3} Mb/s, command --> ");

            return Controls;
        }

        void HandleCommand(string command) {
            if (command == "s") {
                Controls.Plot1.SubSamplePlot = !Controls.Plot1.SubSamplePlot;
                Controls.Plot2.SubSamplePlot = !Controls.Plot2.SubSamplePlot;
            } else if (command == "f") {
                Controls.RunMovements = !Controls.RunMovements;
            } else if (command == "o") {
                Controls.EnableOutput = !Controls.EnableOutput;
            } else if (command == "v") {
                Controls.VideoStandard =
                (Controls.VideoStandard == VideoStandard.Pal10MhzProgessiv
                ? VideoStandard.Pal5MhzProgessiv : VideoStandard.Pal10MhzProgessiv);
            } else if (command == "i") {
                Controls.VideoStandard =
                (Controls.VideoStandard == VideoStandard.Pal5MhzProgessiv
                ? VideoStandard.Pal5MhzInterlaced : VideoStandard.Pal5MhzProgessiv);
            } else if (command == "save") {
                File.WriteAllText(ControlsFile, JsonConvert.SerializeObject(Controls, Formatting.Indented));
            }
        }
    }
}