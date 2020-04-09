using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;


namespace CompositeVideoOscilloscope {

    public class Controller {
        private readonly Action<Controls> Report;
        private readonly Stopwatch Stopwatch;
        public readonly Controls Controls;
        public readonly Movements Movements;

        public Controller(Action<Controls> report, Controls controls = null) {
            Report = report;
            Stopwatch = new Stopwatch();
            Stopwatch.Start();
            Controls = controls ?? new Controls() {
                VideoStandard = VideoStandard_.Pal5MhzProgessiv,
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
            Movements = new Movements();
            Movements.Add(0, -0.04, 0, () => Controls.Plot2.Location.Left, d => Controls.Plot2.Location.Left += d);
            Movements.Add(0, -0.04, 0, () => Controls.Plot2.Location.Top, d => Controls.Plot2.Location.Top += d);
            Movements.Add(Math.PI / 2, 0.4, 0.01, () => Controls.Plot2.Location.Angle, d => Controls.Plot2.Location.Angle += d);
            Movements.Add(-Math.PI / 2, -0.2, -0.01, () => Controls.Plot1.Location.Angle, d => Controls.Plot1.Location.Angle += d);
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
            Report(Controls);
            return Controls;
        }

    }
}