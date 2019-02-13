using System.Diagnostics;
using System.Threading.Tasks;

namespace CompositeVideoOscilloscope {

    public class TimeKeeper {
        readonly Stopwatch StopWatch;
        readonly double MinTime, MaxTime;

        public TimeKeeper(double minTime, double maxTime) {
            MinTime = minTime;
            MaxTime = maxTime;
            StopWatch = new Stopwatch();
            StopWatch.Start();
        }

        async Task Sleep() {
            if (MinTime > 0.001) {
                await Task.Delay((int)(MinTime * 1000));
            } else {
                await Task.Yield();
            }
        }

        double SimulatedTime = 0;
        public async Task<(double, double)> GetElapsedTimeAsync() {

            while (StopWatch.Elapsed.TotalSeconds - SimulatedTime < MinTime) {
                await Sleep();
            }

            var tmp = StopWatch.Elapsed.TotalSeconds;
            var elapsedTime = (tmp - SimulatedTime);
            SimulatedTime = tmp;

            if (elapsedTime > MaxTime) {
                var skipTime = (elapsedTime - MaxTime);
                SimulatedTime += skipTime;
                return (MaxTime, skipTime);
            } else {
                return (elapsedTime, 0);
            }
        }
    }
}
