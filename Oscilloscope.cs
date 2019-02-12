using System.Threading;
using System.Threading.Tasks;

namespace CompositeVideoOscilloscope {
    public class Oscilloscope {
        Output Output;

        public Oscilloscope(Output output) {
            Output = output;
        }

        public async Task Run(CancellationToken canceller) {
            var timing = new PalTiming();
            var timeKeeper = new TimeKeeper(minTime: 200 * timing.DotTime, maxTime: timing.FrameTime);

            var signal = new CompositeSignal(timing);

            double simulatedTime = 0;
            while (!canceller.IsCancellationRequested) {
                var elapsedTime = await timeKeeper.GetElapsedTimeAsync();
                var endTime = simulatedTime + elapsedTime;
                while (simulatedTime < endTime) {
                    Output.Add(signal.Get(simulatedTime));
                    simulatedTime += timing.DotTime;
                }
            }
        }
    }
}
