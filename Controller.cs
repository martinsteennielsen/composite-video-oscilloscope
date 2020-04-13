using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;


namespace CompositeVideoOscilloscope {

    public class Controller {
        public static int MaxPlots = 8;

        public readonly Controls Controls;
        public readonly Queue<string> Commands = new Queue<string>();

        private readonly string ControlsFile;
        private readonly Stopwatch Stopwatch = new Stopwatch();
        private readonly List<Move> Moves = new List<Move>();

        public Controller(string controlsFile = null) {
            ControlsFile = controlsFile;
            Controls = LoadControls() ?? ResetControls(new Controls());
            Stopwatch.Start();
        }

        public async Task<Controls> Run() {
            await Task.Yield();
            var currentTime = Stopwatch.Elapsed.TotalSeconds;
            // var elapsedTime = Controls.VideoStandard.Timing.FrameTime; //currentTime - Controls.CurrentTime;
            var elapsedTime = currentTime - Controls.CurrentTime;
            if (Controls.RunTime) {
                Controls.CurrentTime = currentTime;
            }
            if (Controls.RunMoves) {
                RunMoves(elapsedTime);
            }
            if (Commands.Any()) {
                HandleCommand(Commands.Dequeue());
            }
            return Controls;
        }

        void RunMoves(double elapsedTime) {
            foreach (var move in Moves.ToList()) {
                if (move.Run(elapsedTime)) {
                    Moves.Remove(move);
                }
            }
        }

        void HandleCommand(string command) {
            if (command == "s") {
                Controls.Plots.ForEach(ctrl => ctrl.SubSamplePlot = !ctrl.SubSamplePlot);
            } else if (command == "f") {
                Controls.RunMoves = !Controls.RunMoves;
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
                Controls.Plots.ForEach(ctrl => ctrl.SampleBufferLength *= 2);
            } else if (command == "lessbuf") {
                Controls.Plots.ForEach(ctrl => ctrl.SampleBufferLength /= 2);
            } else if (command == "reset") {
                RunMoves(elapsedTime: -1);
                ResetControls(Controls);
            } else if (command == "time") {
                Controls.RunTime = !Controls.RunTime;
            } else if (command == "interpolate") {
                Controls.Plots.ForEach(ctrl => ctrl.Curve = ctrl.Curve == Curve.Line ? Curve.Stairs : Curve.Line);
            }
        }

        Controls LoadControls() {
            try {
                return JsonConvert.DeserializeObject<Controls>(File.ReadAllText(ControlsFile));
            } catch {
                return null;
            }
        }

        private Controls ResetControls(Controls controls) {
            controls.VideoStandard = VideoStandard.Pal5MhzProgessiv;
            controls.RunMoves = true;
            controls.EnableOutput = true;
            controls.RunTime = true;
            Stopwatch.Restart();
            controls.CurrentTime = 0;
            controls.Plots = DefaultPlots();

            void MoveIt(Expression<Func<Controls, double>> member, double target, double velocity, double acceleration = 0) =>
                Moves.Add(Move.Create(controls, member, target, velocity, acceleration));

            Moves.Clear();
            MoveIt(x => x.Plots[1].Location.Left, 0, -0.04);
            MoveIt(x => x.Plots[1].Location.Top, 0, -0.04);
            MoveIt(x => x.Plots[1].Location.Angle, Math.PI / 2, 0.4, 0.01);
            MoveIt(x => x.Plots[0].Location.Angle, -Math.PI / 2, -0.2, -0.01);

            return controls;
        }

        private static List<PlotControls> DefaultPlots() =>
            new List<PlotControls> {
                new PlotControls {
                    Channel = 0,
                    SampleBufferLength = 1000,
                    SubSamplePlot = false,
                    NumberOfDivisions = 10,
                    Units = (.005, 0.5),
                    Trigger = new TriggerControls { Voltage = 0.6, Edge = 0 },
                    Location = new LocationControls { Left = 0.1, Top = 0.1, Right = 0.5, Bottom = 0.5, Angle = 0 }
                },
                new PlotControls {
                    Channel = 1,
                    SampleBufferLength = 1000,
                    SubSamplePlot = false,
                    NumberOfDivisions = 10,
                    Units = (.005, 0.5),
                    Trigger = new TriggerControls { Voltage = 0.6, Edge = 0 },
                    Location = new LocationControls { Left = 0.6, Top = 0.6, Right = 1.0, Bottom = 1.0, Angle = 0 }
                },
                new PlotControls {
                    Channel = 1,
                    SampleBufferLength = 50,
                    SubSamplePlot = false,
                    NumberOfDivisions = 10,
                    Units = (.0015, 0.4),
                    Trigger = new TriggerControls { Voltage = 0.6, Edge = 0 },
                    Location = new LocationControls { Left = 0.6, Top = 0.6, Right = 1.0, Bottom = 1.0, Angle = 0 }
                }
            };
    }
}
