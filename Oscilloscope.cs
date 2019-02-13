using NetMQ.Sockets;
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
            double simulatedTime = 0;
            var timeKeeper = new TimeKeeper(minTime: 0.25*Timing.FrameTime, maxTime: Timing.FrameTime);
            var signal = new CompositeSignal(Timing);
            while (!canceller.IsCancellationRequested) {
                var (elapsedTime, skipTime) = await timeKeeper.GetElapsedTimeAsync();
                var signalValues = signal.Generate(simulatedTime + elapsedTime, skipTime);
                simulatedTime += elapsedTime;
                Output.Set(signalValues);
            }
        }
    }
}
