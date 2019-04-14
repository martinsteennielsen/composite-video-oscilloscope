using System;
using System.Collections.Generic;

namespace CompositeVideoOscilloscope {

    public class CompositeSignal {
        readonly TimingConstants Timing;
        readonly SignalBlocks[] Frame;
        readonly IScreenContent Content;

        double LastFrameTime = 0;

        struct Signal { public double Value; public double Duration; };
        struct SignalBlocks { public int Count; public Signal[] Signals; public int dy; public int sy; };

        static SignalBlocks[] InterlacedPALFrame(TimingConstants timing) {
            double dark = timing.SyncTimes.BlackLevel, sign = 1d, sync = 0d;

            var synl = new[] {
                new Signal { Value = sync , Duration = 0.5 * timing.LineTime - timing.SyncTimes.LineSyncTime },
                new Signal { Value = dark , Duration = timing.SyncTimes.LineSyncTime } };
            var syns = new[] {
                new Signal { Value = dark , Duration = 0.5 * timing.LineTime - timing.SyncTimes.EquPulseTime },
                new Signal { Value = sync, Duration = timing.SyncTimes.EquPulseTime } };
            var line = new[] {
                new Signal { Value = sync , Duration = timing.SyncTimes.LineSyncTime },
                new Signal { Value = dark , Duration = timing.SyncTimes.LineBlankingTime - timing.SyncTimes.FrontPorchTime - timing.SyncTimes.LineSyncTime },
                new Signal { Value = sign , Duration = timing.LineTime - timing.SyncTimes.LineBlankingTime },
                new Signal { Value = dark , Duration = timing.SyncTimes.FrontPorchTime },
                };
            var blank = new[] {
                new Signal { Value = sync , Duration = timing.SyncTimes.LineSyncTime },
                new Signal { Value = dark , Duration = timing.LineTime - timing.SyncTimes.LineSyncTime },
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

        public CompositeSignal(TimingConstants timing, IScreenContent content) {
            Content = content;
            Timing = timing;
            Frame = InterlacedPALFrame(timing);
        }

        public List<double> Generate(double endTime) {
            if (endTime > LastFrameTime + (2d * Timing.FrameTime)) {
                var (frame, frameDuration) = GenerateFrame();
                LastFrameTime += frameDuration;
                return frame;
            } else {
                return new List<double>();
            }
        }

        (List<double>, double) GenerateFrame() {
            int x = 0, y = 0;
            double time = 0, signalStart = 0;
            double dt = 1d / Timing.BandwidthFreq;
            var frameValues = new List<double>();
            foreach (var block in Frame) {
                y = block.sy;
                for (int i = 0; i < block.Count; i++) {
                    foreach (var signal in block.Signals) {
                        x = 0;
                        for (; time < signalStart + signal.Duration; time += dt) {
                            frameValues.Add(signal.Value == 1d ? Content.PixelValue(x,y) : signal.Value);
                            x++;
                        }
                        signalStart += signal.Duration;
                    }
                    y += block.dy;
                }
            }
            return (frameValues, time);
        }
    }
}
