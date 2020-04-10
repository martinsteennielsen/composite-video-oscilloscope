using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;


namespace CompositeVideoOscilloscope {

    public class Controller {
        public readonly Controls Controls;
        public readonly Queue<string> Commands = new Queue<string>();

        private readonly string ControlsFile;
        private readonly Stopwatch Stopwatch = new Stopwatch();
        private readonly Movements Movements;

        public Controller(string controlsFile = null) {
            ControlsFile = controlsFile;
            Controls = LoadControls() ?? ResetControls(new Controls());
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

        private static Controls ResetControls(Controls controls) {
            controls.VideoStandard = VideoStandard.Pal5MhzProgessiv;
            controls.Plot1 = new PlotControls {
                SampleBufferLength = 1000,
                SubSamplePlot = false,
                NumberOfDivisions = 10,
                Units = (.005, 0.5),
                Trigger = new TriggerControls { Voltage = 0.6, Edge = 0 },
                Location = new LocationControls { Left = 0.1, Top = 0.1, Right = 0.5, Bottom = 0.5, Angle = 0 }
            };
            controls.Plot2 = new PlotControls {
                SampleBufferLength = 1000,
                SubSamplePlot = false,
                NumberOfDivisions = 10,
                Units = (.005, 0.5),
                Trigger = new TriggerControls { Voltage = 0.6, Edge = 0 },
                Location = new LocationControls { Left = 0.6, Top = 0.6, Right = 1.0, Bottom = 1.0, Angle = 0 }
            };
            controls.RunMovements = true;
            controls.EnableOutput = true;
            return controls;
        }

        public async Task<Controls> Run() {
            await Task.Yield();
            var currentTime = Stopwatch.Elapsed.TotalSeconds;
            // var elapsedTime = Controls.VideoStandard.Timing.FrameTime; //currentTime - Controls.CurrentTime;
            var elapsedTime = currentTime - Controls.CurrentTime;
            Controls.CurrentTime = currentTime;

            if (Controls.RunMovements) {
                Movements.Run(Controls, elapsedTime);
            }
            if (Commands.Any()) {
                HandleCommand(Commands.Dequeue());
            }
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
            } else if (command == "morebuf") {
                Controls.Plot1.SampleBufferLength *= 2;
                Controls.Plot2.SampleBufferLength *= 2;
            } else if (command == "lessbuf") {
                Controls.Plot1.SampleBufferLength /= 2;
                Controls.Plot2.SampleBufferLength /= 2;
            } else if (command == "reset") {
                ResetControls(Controls);
            }
        }
    }
}
