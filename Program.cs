using System.Threading.Tasks;

namespace CompositeVideoOscilloscope
{
    class Program {
        static async Task Main(string[] args) {
            var timer = new Timer(new PalTiming());
            double simulatedTime = 0;
            while (true) {
                simulatedTime += await timer.GetElapsedTimeAsync();
                // TODO composite signal generation
            }
        }
    }
}
