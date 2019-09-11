using System;
using System.Collections.Generic;

namespace CompositeVideoOscilloscope {

    public interface IContent {
        int Intensity(int x, int y);
    }

    public class VideoSignal {
        const int ns = 10000000;
        double LastFrameTime = 0;

        public List<int> Generate(double time, VideoStandard standard, IContent content) {
            if (time > LastFrameTime + (2d * standard.Timing.FrameTime)) {
                var (frame, frameDuration) = GenerateFrame(standard, content);
                LastFrameTime += frameDuration;
                return frame;
            } else {
                return new List<int>();
            }
        }

        (List<int>, double) GenerateFrame(VideoStandard standard, IContent content) {

            int intensity(int sx, int sy) {
                int val = content.Intensity(sx,sy);
                int vres = val * (255 - standard.BlackLevel);
                vres /= 255;
                vres += standard.BlackLevel;
                vres = vres > 255 ? 255 : Math.Max(standard.BlackLevel, vres);
                return val == 0 ? standard.BlackLevel : vres;
            }

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
                            frameValues.Add(signal.Value == 255 ? intensity(x,y) : signal.Value);
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
