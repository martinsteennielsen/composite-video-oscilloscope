using System.Threading;
using System.Threading.Tasks;

namespace CompositeVideoOscilloscope {
    public class Oscilloscope {
        Output Output;
        Timing Timing;

        public Oscilloscope(Timing timing, Output output) {
            Output = output;
            Timing = timing;
        }

        public async Task Run(CancellationToken canceller) {
            var timeKeeper = new TimeKeeper(minTime: 200 * Timing.DotTime, maxTime: Timing.FrameTime);

            var signal = new CompositeSignal(Timing);

            double simulatedTime = 0;
            while (!canceller.IsCancellationRequested) {
                var elapsedTime = await timeKeeper.GetElapsedTimeAsync();
                var endTime = simulatedTime + elapsedTime;
                while (simulatedTime < endTime) {
                    Output.Add(signal.Get(simulatedTime));
                    simulatedTime += Timing.DotTime;
                }
            }
        }
    }
}
