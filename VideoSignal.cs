using System;
using System.Collections.Generic;

namespace CompositeVideoOscilloscope {

    public class VideoSignal {
        const int ns = 10000000;
        double LastFrameTime = 0;

        public List<int> Generate(double time, VideoStandard standard, IScreenContent content) {
            if (time > LastFrameTime + (2d * standard.Timing.FrameTime)) {
                var (frame, frameDuration) = GenerateFrame(standard, content);
                LastFrameTime += frameDuration;
                return frame;
            } else {
                return new List<int>();
            }
        }

        (List<int>, double) GenerateFrame(VideoStandard standard, IScreenContent content) {
            int x = 0, y = 0;
            int time = 0, signalStart = 0;
            int dt = (int)(ns / standard.Timing.BandwidthFreq);
            var frameValues = new List<int>();
            foreach (var block in standard.Blocks) {
                y = block.sy;
                for (int i = 0; i < block.Count; i++) {
                    foreach (var signal in block.Signals) {
                        x = 0;
                        for (; time < signalStart + signal.Duration; time += dt) {
                            frameValues.Add(signal.Value == 255 ? content.PixelValue(x,y) : signal.Value);
                            x++;
                        }
                        signalStart += signal.Duration;
                    }
                    y += block.dy;
                }
            }
            return (frameValues, time / ns);
        }
    }
}
