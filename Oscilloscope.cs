using System.Threading;
using System.Threading.Tasks;

namespace CompositeVideoOscilloscope {
    public class Oscilloscope {
        readonly Aquisition Aquisition;
        readonly Controller Controller;
        readonly Output Output;
        readonly Screen Screen;

        public Oscilloscope(Output output) {
            Output = output;
            Controller = new Controller(); 
            Aquisition = new Aquisition();
            Screen = new Screen(Controller, aquisition: Aquisition);
        }

        public async Task Run(CancellationToken canceller) {
            var videoSignal = new VideoSignal();
            while (!canceller.IsCancellationRequested) {
                var controls = await Controller.Run().ConfigureAwait(false);
                Output.Send(videoSignal.Generate(controls.CurrentTime, controls.VideoStandard, content: Screen.Content), sampleRate: controls.VideoStandard.Timing.BandwidthFreq);
            }
        }
    }
}
