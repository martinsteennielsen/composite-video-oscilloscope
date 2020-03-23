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

        public async Task<Controls> Run(int noOfGeneratedBytes) {
            await Task.Yield();
            var currentTime = Stopwatch.Elapsed.TotalSeconds;
            // var elapsedTime = Controls.VideoStandard.Timing.FrameTime; //currentTime - Controls.CurrentTime;
            var elapsedTime = currentTime - Controls.CurrentTime;
            Controls.TimeMsCount += (int)(1000*elapsedTime);
            if (Controls.TimeMsCount > 200) {
                Controls.BytesPrSecond = 1000.0*Controls.ByteCount / Controls.TimeMsCount;
                Controls.ByteCount = noOfGeneratedBytes;
                Controls.TimeMsCount = Controls.TimeMsCount % 200;
            } else {
                Controls.ByteCount += noOfGeneratedBytes;
            }

            Controls.CurrentTime = currentTime;
            Movements.Run(Controls, elapsedTime);
            Report();
            return Controls;
        }

        private void Report()
        {
            Console.SetCursorPosition(0, 0);
            Console.WriteLine($"{Controls.BytesPrSecond/1e6:F3} Mb/s ");
        }
    }
}