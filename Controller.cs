using System.Diagnostics;
using System.Threading.Tasks;


namespace CompositeVideoOscilloscope {

    public class Controller {
        private readonly Stopwatch Stopwatch;

        public readonly static Controls StartupControls = 
            new Controls() {
                 NumberOfDivisions = 10,
                 ScreenPosition =  (0.1,0.1,.9,.9),
                 Units =  (5, 0.5),
                 VideoStandard = VideoStandard.Pal5MhzInterlaced,
                 TriggerVoltage = 0.7
            };

        public Controller() {
            Stopwatch = new Stopwatch();
            Stopwatch.Start();
        }

        public async Task<Controls> Run(Controls controls) {
            await Task.Delay((int)(0.5 * 1000 * controls.VideoStandard.Timing.FrameTime)).ConfigureAwait(false);
            var currentTime = Stopwatch.Elapsed.TotalSeconds;
            controls.ElapsedTime = currentTime - controls.CurrentTime;
            controls.CurrentTime = currentTime;
            return controls;
        }
    }
}