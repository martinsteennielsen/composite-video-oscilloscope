using System;
using System.Linq;
using System.Collections.Generic;

namespace CompositeVideoOscilloscope {

    public class CompositeSignal {
        readonly TimingConstants Timing;
        readonly SignalBlocks[] Frame;

        double LastFrameTime = 0;

        struct Signal { public double Value; public double Duration; };
        struct SignalBlocks { public int Count; public Signal[] Signals; };

        static SignalBlocks[] InterlacedPALFrame(TimingConstants timing) {
            double dark = 0.3, sign = 1d, sync = 0d;

            var synl = new[] {
                new Signal { Value = sync , Duration = 0.5 * timing.LineTime - timing.SyncTimes.LineSyncTime },
                new Signal { Value = dark , Duration = timing.SyncTimes.LineSyncTime } };
            var syns = new[] {
                new Signal { Value = sync , Duration = 0.5 * timing.LineTime - timing.SyncTimes.EquPulseTime },
                new Signal { Value = dark , Duration = timing.SyncTimes.EquPulseTime } };
            var blank = new[] {
                new Signal { Value = dark , Duration = timing.SyncTimes.FrontPorchTime },
                new Signal { Value = sync , Duration = timing.SyncTimes.LineSyncTime },
                new Signal { Value = dark , Duration = timing.SyncTimes.LineBlankingTime - timing.SyncTimes.FrontPorchTime - timing.SyncTimes.LineSyncTime },
                new Signal { Value = dark , Duration = timing.LineTime - timing.SyncTimes.LineBlankingTime } };
            var line = new[] {
                new Signal { Value = dark , Duration = timing.SyncTimes.FrontPorchTime },
                new Signal { Value = sync , Duration = timing.SyncTimes.LineSyncTime },
                new Signal { Value = dark , Duration = timing.SyncTimes.LineBlankingTime - timing.SyncTimes.FrontPorchTime - timing.SyncTimes.LineSyncTime },
                new Signal { Value = sign , Duration = timing.LineTime - timing.SyncTimes.LineBlankingTime } };
            var firstHalf = new[] {
                new Signal  { Value = sign , Duration = 0.5 * timing.LineTime} };

            var secondHalf = new[] {
                new Signal { Value = dark , Duration = timing.SyncTimes.FrontPorchTime },
                new Signal { Value = sync , Duration = timing.SyncTimes.LineSyncTime },
                new Signal { Value = dark , Duration = timing.SyncTimes.LineBlankingTime - timing.SyncTimes.FrontPorchTime - timing.SyncTimes.LineSyncTime },
                new Signal { Value = sign , Duration = 0.5 * timing.LineTime - timing.SyncTimes.LineBlankingTime } };

            return new[] {
                new SignalBlocks { Count = 5  , Signals = synl }, new SignalBlocks { Count = 5  , Signals = syns }, new SignalBlocks { Count = 12 , Signals = blank },
                new SignalBlocks { Count = 1,   Signals = firstHalf },
                new SignalBlocks { Count = 293, Signals = line },
                new SignalBlocks { Count = 5  , Signals = syns }, new SignalBlocks { Count = 5  , Signals = synl }, new SignalBlocks { Count = 4  , Signals = syns }, new SignalBlocks { Count = 12 , Signals = blank },
                new SignalBlocks { Count = 292, Signals = line },
                new SignalBlocks { Count = 1,   Signals = secondHalf },
                new SignalBlocks { Count = 6  , Signals = syns },
            };
        }

        public CompositeSignal(TimingConstants timing) {
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
            double time = 0, signalStart = 0;
            double dt = 1d / Timing.BandwidthFreq;
            var frameValues = new List<double>();
            foreach (var block in Frame) {
                double blockDuration = 0;
                for (int i = 0; i < block.Count; i++) {
                    foreach (var signal in block.Signals) {
                        for (; time < signalStart + signal.Duration; time += dt) {
                            frameValues.Add(signal.Value == 1d ? PixelValue(time) : signal.Value);
                            blockDuration += (1e6 * dt);
                        }
                        signalStart += signal.Duration;
                    }
                }
            }
            return (frameValues, time);
        }

        double PixelValue(double simulatedTime) {
            double w = Timing.LineTime / Timing.DotTime;
            int x = (int)(simulatedTime % Timing.LineTime / Timing.DotTime);
            int y = (int)(simulatedTime % (Timing.FrameTime) / Timing.LineTime);
            double step(double v) => (int)(10d * (double)v / w) / 10.0;
            return step(y) * step(x) * 0.7 + 0.3;
        }

    }
}
