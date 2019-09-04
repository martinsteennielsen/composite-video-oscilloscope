using System;
using System.Diagnostics;
using System.Threading.Tasks;


namespace CompositeVideoOscilloscope {

    public class Controller {
        private readonly Movements Movements;
        private readonly Stopwatch Stopwatch;
        private readonly Controls Controls = 
            new Controls() {
                 NumberOfDivisions = 10,
                 ScreenPosition =  (0.4,0.1,.9,.9),
                 Units =  (.005, 0.5),
                 VideoStandard = VideoStandard.Pal5MhzProgessiv,
                 TriggerVoltage = 0.6,
                 TriggerEdge = 0,
                 Angle=0
            };

        public Controller() {
            Movements = new Movements();
            Movements.Add(Math.PI / 2, 0.1, 0.0000001, () => Controls.Angle, d => Controls.Angle += d);
            Stopwatch = new Stopwatch();
            Stopwatch.Start();
        }

        public async Task<Controls> Run() {
            await Task.Delay((int)(0.5 * 1000 * Controls.VideoStandard.Timing.FrameTime)).ConfigureAwait(false);
            var currentTime = Stopwatch.Elapsed.TotalSeconds;
            // var elapsedTime = Controls.VideoStandard.Timing.FrameTime; //currentTime - Controls.CurrentTime;
            var elapsedTime = currentTime - Controls.CurrentTime;
            Controls.CurrentTime = currentTime;
            Movements.Run(Controls, elapsedTime);
            return Controls;
        }
    }
}