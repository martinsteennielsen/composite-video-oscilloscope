using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace CompositeVideoOscilloscope {
    public class Oscilloscope {
        readonly InputSignal InputSignal;
        readonly CompositeSignal CompositeSignal;
        readonly TimingConstants Timing;
        readonly Controller Controller;
        readonly Output Output;
        readonly Stopwatch StopWatch;
        public Oscilloscope(TimingConstants timing, Output output) {
            Output = output;
            Timing = timing;
            Controller = new Controller(); 
            InputSignal = new InputSignal();
            CompositeSignal = new CompositeSignal(Timing);
            StopWatch = new Stopwatch();
        }

        public async Task Run(CancellationToken canceller) {
            StopWatch.Start();
            var controls = Controller.StartupControls;
            while (!canceller.IsCancellationRequested) {
                var elapsed = await Relax(StopWatch).ConfigureAwait(false);
                controls = Controller.Run(controls, elapsed);
                Output.Send(CompositeSignal.Generate(elapsed, new ScreenContent(Timing, controls, signal: InputSignal)));
            }
        }

        private async Task<double> Relax(Stopwatch sw) {
            await Task.Delay((int)(0.5 * 1000 * Timing.FrameTime)).ConfigureAwait(false);
            return sw.Elapsed.TotalSeconds;
        }
    }
}
