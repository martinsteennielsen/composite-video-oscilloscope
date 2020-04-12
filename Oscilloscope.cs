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

        public Oscilloscope(Output output, Controller controller) {
            Output = output;
            Controller = controller;
            Aquisition = new Aquisition();
            Screen = new Screen(Controller, aquisition: Aquisition);
        }

        public async Task Run(CancellationToken canceller) {

            var runningAverage = new Queue<(double, int)>();

            double bps(double time, int length) {
                runningAverage.Enqueue((time, length));
                if (runningAverage.Count > 50) { runningAverage.Dequeue(); }
                return runningAverage.Sum(x => x.Item2) / (runningAverage.Max(x => x.Item1) - runningAverage.Min(x => x.Item1));
            }

            while (!canceller.IsCancellationRequested) {
                var controls = await Controller.Run().ConfigureAwait(false);
                var videoConstants = VideoConstants.Get(controls.VideoStandard);
                var frame = new VideoFrame(videoConstants, Screen.Content).Get();
                if (controls.EnableOutput) {
                    Output.Send(frame, sampleRate: videoConstants.Timing.BandwidthFreq);
                }
                var _bps = bps(controls.CurrentTime, frame.Length);
                Console.Write($"\r {_bps / 1e6:F3} Mb/s, {_bps / frame.Length:F0} fps, command --> ");
            }
        }
    }
}
