using System.Diagnostics;
using System.Threading.Tasks;


namespace CompositeVideoOscilloscope {

    public class Controller {
        private readonly Stopwatch Stopwatch;

        public readonly static Controls StartupControls = 
            new Controls()
                .WithUnits(timePrDivision: 5, voltagePrDivision: 0.5)
                .WithDivisions(8)
                .WithTiming(TimingConstants.Pal);

        public Controller() {
            Stopwatch = new Stopwatch();
            Stopwatch.Start();
        }

        public async Task<Controls> Run(Controls controls) {
            await Task.Delay((int)(0.5 * 1000 * controls.Timing.FrameTime)).ConfigureAwait(false);
            return controls.WithTime(Stopwatch.Elapsed.TotalSeconds);
        }
    }
}