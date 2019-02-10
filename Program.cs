using System.Threading.Tasks;

namespace CompositeVideoOscilloscope {
    class Program {
        static async Task Main(string[] args) {
            var timing = new PalTiming();
            var timer = new Timer(minTime: 2 * timing.DotTime, maxTime: timing.FrameTime);
            double simulatedTime = 0;
            while (true) {
                simulatedTime += await timer.GetElapsedTimeAsync();
                // TODO sample input
                // TODO generate picture
                // TODO generate composite signal
            }
        }
    }
}