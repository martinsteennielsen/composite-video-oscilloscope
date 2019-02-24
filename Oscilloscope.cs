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
            var signal = new CompositeSignal(Timing);
            var sw = new Stopwatch();
            sw.Start();
            while (!canceller.IsCancellationRequested) {
                await Task.Delay(((int)(0.5 * 1000 * Timing.FrameTime)));
                Output.Set(signal.Generate(sw.Elapsed.TotalSeconds));
            }
        }
    }
}
