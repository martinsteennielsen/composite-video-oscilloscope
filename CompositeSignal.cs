using System;
using System.Collections.Generic;

namespace CompositeVideoOscilloscope {
    public class CompositeSignal {
        readonly TimingConstants Timing;
        readonly Random Randomizer = new Random();
        readonly ISignal HSync, HBlank;

        public CompositeSignal(TimingConstants timing) {
            Timing = timing;
            HBlank = new SquareSignal(frequency: Timing.HFreq, onTime: Timing.SyncTimes.LineBlankingTime);
            HSync = new SquareSignal(frequency: Timing.HFreq, onTime: Timing.SyncTimes.LineSyncTime, onStartTime: Timing.SyncTimes.FrontPorchTime);
        }

        public double Get(double time) => Randomizer.NextDouble();

        private double SimulatedTime = 0;

        public List<double> Generate(double endTime, double skipTime) {
            SimulatedTime += skipTime;
            endTime += skipTime;
            var result = new List<double>();
            int startX = (int)(SimulatedTime % Timing.LineTime / Timing.DotTime);
            int startY = (int)(SimulatedTime % Timing.FrameTime / Timing.LineTime);
            int x = startX;
            int y = startY;
            while (SimulatedTime < endTime) {
                double val = 0.3 + 0.7 * Get(x * Timing.DotTime + y * Timing.LineTime);
                if (HBlank.Get(SimulatedTime) == 1) {
                    val = HSync.Get(SimulatedTime) == 1 ? 0 : 0.3;
                }
                if (y > 300) { val = 0; }
                result.Add(val);
                x++; if (x > Timing.LineTime / Timing.DotTime) { x = 0; y++; }
                if (y > Timing.FrameTime / Timing.LineTime) { y = 0; }
                SimulatedTime += Timing.DotTime;
            }
            return result;
        }
    }
}
