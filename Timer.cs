using System.Diagnostics;
using System.Threading.Tasks;

namespace CompositeVideoOscilloscope {

    public class Timer {
        readonly Timing Timing;
        readonly Stopwatch StopWatch;
        double SimulatedTime;

        public Timer(Timing timing) {
            Timing = timing;
            StopWatch = new Stopwatch();
            StopWatch.Start();
            SimulatedTime = 0;
        }

        public async Task<double> GetElapsedTimeAsync() {
            double minTime = 2 * Timing.DotTime;

            while (StopWatch.Elapsed.TotalSeconds - SimulatedTime < minTime) {
                if (minTime > 0.001) {
                    await Task.Delay((int)(minTime * 1000));
                } else {
                    await Task.Yield();
                }
            }

            var tmpTime = StopWatch.Elapsed.TotalSeconds;
            var elapsedTime = (tmpTime - SimulatedTime);
            SimulatedTime = tmpTime;

            if (elapsedTime > 2.0 * Timing.FrameTime) {
                var skipTime = (elapsedTime - Timing.FrameTime);
                SimulatedTime += skipTime;
                elapsedTime = Timing.FrameTime;
            }
            
            return elapsedTime;
        }
    }
}
