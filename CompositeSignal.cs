using System;
using System.Collections.Generic;

namespace CompositeVideoOscilloscope {
    public class CompositeSignal {
        readonly TimingConstants Timing;
        readonly Random Randomizer = new Random();
        readonly ISignal HSync, HBlank, VSync;

        public CompositeSignal(TimingConstants timing) {
            Timing = timing;
            HBlank = new SquareSignal(frequency: Timing.HFreq, onTime: Timing.SyncTimes.LineBlankingTime);
            HSync = new SquareSignal(frequency: Timing.HFreq, onTime: Timing.SyncTimes.LineSyncTime, onStartTime: Timing.SyncTimes.FrontPorchTime);
            VSync = new SquareSignal(frequency: Timing.VFreq, onTime: Timing.FrameTime - 50 * Timing.LineTime);
        }

        public double Get(double time) => Randomizer.NextDouble();

        private double SimulatedTime = 0;

        double? SyncValue(double simulatedTime) {

            bool isHBlank() => HBlank.Get(simulatedTime) == 1;
            bool isVBlank() => VSync.Get(SimulatedTime) == 1;

            double? syncValue(double t) =>
                isHBlank() ? (double?)(HSync.Get(t) == 1 ? 0 : 0.3) : null;

            if (isVBlank()) {
                return syncValue(SimulatedTime);
            } else {
                return syncValue(SimulatedTime) ?? 0.3;
            }
        }


        double PixelValue(double simulatedTime) {
            double w = Timing.LineTime / Timing.DotTime;
            int x = (int)(SimulatedTime % Timing.LineTime / Timing.DotTime);
            int y = (int)(SimulatedTime % Timing.FrameTime / Timing.LineTime);
            double step(double v) => Math.Ceiling(10.0 * (double)v / w) / 10.0;
            return step(y) * step(x) * 0.7 + 0.3;
        }

        public List<double> Generate(double endTime, double skipTime) {
            SimulatedTime += skipTime;
            endTime += skipTime;
            var result = new List<double>();
            while (SimulatedTime < endTime) {
                double signalValue = SyncValue(SimulatedTime) ?? PixelValue(SimulatedTime);
                result.Add(signalValue);
                SimulatedTime += Timing.DotTime;
            }
            return result;
        }
    }
}
