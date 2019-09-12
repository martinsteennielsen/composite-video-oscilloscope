using System.Threading;
using System.Threading.Tasks;

namespace CompositeVideoOscilloscope {
    public class Oscilloscope {
        readonly InputSignal InputSignal;
        readonly Controller Controller;
        readonly Output Output;
        readonly Screen Screen;

        public Oscilloscope(Output output) {
            Output = output;
            Controller = new Controller(); 
            InputSignal = new InputSignal();
            Screen = new Screen(Controller, signal: InputSignal);
        }

        public async Task Run(CancellationToken canceller) {
            var videoSignal = new VideoSignal();
            while (!canceller.IsCancellationRequested) {
                var controls = await Controller.Run().ConfigureAwait(false);
                Output.Send(videoSignal.Generate(controls.CurrentTime, controls.VideoStandard, content: Screen.Content));
            }
        }
    }
}
