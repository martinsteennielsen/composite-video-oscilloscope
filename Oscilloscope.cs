using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace CompositeVideoOscilloscope {
    public class Oscilloscope {
        readonly Output Output;
        readonly TimingConstants Timing;
        public Oscilloscope(TimingConstants timing, Output output) {
            Output = output;
            Timing = timing;
        }

        public async Task Run(CancellationToken canceller) {
            var controls = new Controls().WithUnits(timePrDivision: 5, voltagePrDivision: 0.5);
            var content = new ScreenContent(Timing, controls, signal: new InputSignal());
            var signal = new CompositeSignal(Timing, content);
            var sw = new Stopwatch();
            sw.Start();
            while (!canceller.IsCancellationRequested) {
                await Task.Delay((int)(0.5 * 1000 * Timing.FrameTime)).ConfigureAwait(false);
                controls = controls.ElapseTime(sw.Elapsed.TotalSeconds);
                Output.Set(signal.Generate(controls));
            }
        }
    }
}
