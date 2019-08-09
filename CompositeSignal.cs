using System;
using System.Collections.Generic;

namespace CompositeVideoOscilloscope {

    public class CompositeSignal {
        const int ns = 10000000;
        readonly SignalBlocks[] Frame;
        double LastFrameTime = 0;

        struct Signal { public int Value; public int Duration; };
        struct SignalBlocks { public int Count; public Signal[] Signals; public int dy; public int sy; };

        static SignalBlocks[] InterlacedPALFrame(TimingConstants timing) {
            int dark = timing.SyncTimes.BlackLevel, sign = 255, sync = 0;

            var synl = new[] {
                new Signal { Value = sync , Duration = (int)(ns * (0.5 * timing.LineTime - timing.SyncTimes.LineSyncTime)) },
                new Signal { Value = dark , Duration = (int)(ns * timing.SyncTimes.LineSyncTime )} };
            var syns = new[] {
                new Signal { Value = dark , Duration = (int)(ns * (0.5 * timing.LineTime - timing.SyncTimes.EquPulseTime)) },
                new Signal { Value = sync,  Duration = (int)(ns * timing.SyncTimes.EquPulseTime) } };
            var line = new[] {
                new Signal { Value = sync , Duration = (int)(ns * timing.SyncTimes.LineSyncTime) },
                new Signal { Value = dark , Duration = (int)(ns * (timing.SyncTimes.LineBlankingTime - timing.SyncTimes.FrontPorchTime - timing.SyncTimes.LineSyncTime)) },
                new Signal { Value = sign , Duration = (int)(ns * (timing.LineTime - timing.SyncTimes.LineBlankingTime)) },
                new Signal { Value = dark , Duration = (int)(ns * timing.SyncTimes.FrontPorchTime) },
                };
            var blank = new[] {
                new Signal { Value = sync , Duration = (int)(ns * timing.SyncTimes.LineSyncTime) },
                new Signal { Value = dark , Duration = (int)(ns * (timing.LineTime - timing.SyncTimes.LineSyncTime)) },
                };

            return new[] {
                new SignalBlocks { Count = 5  , Signals = synl }, new SignalBlocks { Count = 5  , Signals = syns },
                new SignalBlocks { Count = 12 , Signals = blank },
                new SignalBlocks { Count = 293, Signals = line, dy = 2, sy = 1 },
                new SignalBlocks { Count = 5  , Signals = syns }, new SignalBlocks { Count = 5  , Signals = synl }, new SignalBlocks { Count = 4  , Signals = syns },
                new SignalBlocks { Count = 12 , Signals = blank },
                new SignalBlocks { Count = 293, Signals = line, dy = 2, sy = 0 },
                new SignalBlocks { Count = 6  , Signals = syns },
            };
        }

        public CompositeSignal(TimingConstants timing) {
            Frame = InterlacedPALFrame(timing);
        }

        public List<int> Generate(double time, TimingConstants timing, IScreenContent content) {
            if (time > LastFrameTime + (2d * timing.FrameTime)) {
                var (frame, frameDuration) = GenerateFrame(timing, content);
                LastFrameTime += frameDuration;
                return frame;
            } else {
                return new List<int>();
            }
        }

        (List<int>, double) GenerateFrame(TimingConstants timing, IScreenContent content) {
            content.VSync();
            int x = 0, y = 0;
            int time = 0, signalStart = 0;
            int dt = (int)(ns / timing.BandwidthFreq);
            var frameValues = new List<int>();
            foreach (var block in Frame) {
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
