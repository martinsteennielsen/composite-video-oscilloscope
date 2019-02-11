using System.Threading;
using System.Threading.Tasks;

namespace CompositeVideoOscilloscope {
    class Program {

        static async Task Main(string[] args) {
            var output = new Output();
            var canceller = new CancellationTokenSource().Token;
            await Task.WhenAny(
                Run(canceller, output), 
                output.Run(canceller)
                );
        }

        static async Task Run(CancellationToken canceller, Output output) {
            var timing = new PalTiming();
            var signal = new CompositeSignal(timing);
            var timer = new Timer(minTime: 200 * timing.DotTime, maxTime: timing.FrameTime);

            double simulatedTime = 0;
            while (!canceller.IsCancellationRequested) {
                var elapsedTime = await timer.GetElapsedTimeAsync();
                var endTime = simulatedTime + elapsedTime;
                while (simulatedTime < endTime) {
                    output.Add(signal.Get(simulatedTime));
                    simulatedTime += timing.DotTime;
                }
            }
        }
    }
}