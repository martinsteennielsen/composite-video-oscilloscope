using System.Diagnostics;
using System.Threading.Tasks;


namespace CompositeVideoOscilloscope {

    public class Controller {
        private readonly Stopwatch Stopwatch;
        private readonly Controls Controls = 
            new Controls() {
                 NumberOfDivisions = 10,
                 ScreenPosition =  (0,0,1,1),
                 Units =  (.005, 0.5),
                 VideoStandard = VideoStandard.Pal5MhzInterlaced,
                 TriggerVoltage = 0.6,
                 TriggerEdge = 0
            };

        public Controller() {
            Stopwatch = new Stopwatch();
            Stopwatch.Start();
        }

        public async Task<Controls> Run() {
            await Task.Delay((int)(0.5 * 1000 * Controls.VideoStandard.Timing.FrameTime)).ConfigureAwait(false);
            var currentTime = Stopwatch.Elapsed.TotalSeconds;
            Controls.ElapsedTime = currentTime - Controls.CurrentTime;
            Controls.CurrentTime = currentTime;
            return Controls;
        }
    }
}