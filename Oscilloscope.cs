using System.Threading;
using System.Threading.Tasks;

namespace CompositeVideoOscilloscope {
    public class Oscilloscope {
        readonly Aquisition Aquisition;
        readonly Controller Controller;
        readonly Output Output;
        readonly Screen Screen;

        public Oscilloscope(Output output, Controller controller) {
            Output = output;
            Controller = controller;
            Aquisition = new Aquisition();
            Screen = new Screen(Controller, aquisition: Aquisition);
        }

        public async Task Run(CancellationToken canceller) {
            var controls = await Controller.Run(noOfGeneratedBytes: 0);
            while (!canceller.IsCancellationRequested) {
                var frame = new VideoFrame(controls.VideoStandard, Screen.Content).Get();
                if (controls.EnableOutput) {
                    Output.Send(frame, sampleRate: controls.VideoStandard.Timing.BandwidthFreq);
                }
                controls = await Controller.Run(noOfGeneratedBytes: frame.Length).ConfigureAwait(false);
            }
        }
    }
}
