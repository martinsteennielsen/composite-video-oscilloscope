using System.Diagnostics;
using System.Threading.Tasks;

namespace CompositeVideoOscilloscope {

    public class Timer {
        readonly Stopwatch StopWatch;
        readonly double MinTime, MaxTime;
        double SimulatedTime;

        public Timer(double minTime, double maxTime) {
            MinTime = minTime;
            MaxTime = maxTime;
            StopWatch = new Stopwatch();
            StopWatch.Start();
            SimulatedTime = 0;
        }

        public async Task<double> GetElapsedTimeAsync() {
            while (StopWatch.Elapsed.TotalSeconds - SimulatedTime < MinTime) {
                if (MinTime > 0.001) {
                    await Task.Delay((int)(MinTime * 1000));
                } else {
                    await Task.Yield();
                }
            }

            var tmpTime = StopWatch.Elapsed.TotalSeconds;
            var elapsedTime = (tmpTime - SimulatedTime);
            SimulatedTime = tmpTime;

            if (elapsedTime > 2.0 * MaxTime) {
                var skipTime = (elapsedTime - MaxTime);
                SimulatedTime += skipTime;
                elapsedTime = MaxTime;
            }

            return elapsedTime;
        }
    }
}
