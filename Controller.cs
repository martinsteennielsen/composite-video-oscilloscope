using System;
using System.Diagnostics;
using System.Threading.Tasks;


namespace CompositeVideoOscilloscope {

    public class Controller {
        public readonly Movements Movements;
        private readonly Stopwatch Stopwatch;
        public readonly Controls Controls = 
            new Controls() {
                 VideoStandard = VideoStandard.Pal5MhzProgessiv,
                 PlotControls =  new PlotControls {  
                        NumberOfDivisions = 10, 
                        Units =  (.005*1e9, 0.5), 
                        Trigger = new TriggerControls { Voltage = 0.6, Edge = 0 } }
            };

        public Controller() {
            Movements = new Movements();
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