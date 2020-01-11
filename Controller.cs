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
                        Units =  (.005, 0.5), 
                        Trigger = new TriggerControls { Voltage = 0.6, Edge = 0 } }
            };

        public Controller() {
            Movements = new Movements();
            Stopwatch = new Stopwatch();
            Stopwatch.Start();
        }

        public async Task<Controls> Run() {
            Report();
            await Task.Yield();
            var currentTime = Stopwatch.Elapsed.TotalSeconds;
            // var elapsedTime = Controls.VideoStandard.Timing.FrameTime; //currentTime - Controls.CurrentTime;
            var elapsedTime = currentTime - Controls.CurrentTime;
            Controls.CurrentTime = currentTime;
            Movements.Run(Controls, elapsedTime);
            return Controls;
        }

        private void Report() {
            var row = Console.CursorTop;
            Console.WriteLine($"{(Controls.BytesGenerated / Controls.CurrentTime * 1e-6):F3} Mb/s\n");
            Console.SetCursorPosition(0, row);
        }
    }
}