using System;
using System.Collections.Generic;
using System.Linq;
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
                var frame = videoSignal.GenerateFrame(controls.VideoStandard, Screen.Content).SelectMany(x=>x).ToArray();
                Output.Send(frame, sampleRate: controls.VideoStandard.Timing.BandwidthFreq);
                controls.BytesGenerated += frame.Length;
            }
        }
    }
}
