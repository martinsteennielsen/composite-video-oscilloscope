using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace CompositeVideoOscilloscope {
    public class Oscilloscope {
        readonly InputSignal InputSignal;
        readonly Controller Controller;
        readonly Output Output;
        public Oscilloscope(Output output) {
            Output = output;
            Controller = new Controller(); 
            InputSignal = new InputSignal();
        }

        public async Task Run(CancellationToken canceller) {
            var controls = Controller.StartupControls;
            var compositeSignal = new CompositeSignal(controls.Timing);
            while (!canceller.IsCancellationRequested) {
                controls = await Controller.Run(controls).ConfigureAwait(false);
                Output.Send(compositeSignal.Generate(controls.CurrentTime, controls.Timing, new ScreenContent(controls, signal: InputSignal)));
            }
        }
    }
}
