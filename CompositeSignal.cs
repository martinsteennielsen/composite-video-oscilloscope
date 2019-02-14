using System;
using System.Collections.Generic;

namespace CompositeVideoOscilloscope {
    public class CompositeSignal {
        readonly TimingConstants Timing;
        readonly ISignal HSync, HBlank, VBlank, VEqu, VEquOn, VSyncOn, VSync;

        public CompositeSignal(TimingConstants timing) {
            Timing = timing;
            HBlank = new SquareSignal(frequency: Timing.HFreq, onTime: Timing.SyncTimes.LineBlankingTime);
            VBlank = new SquareSignal(frequency: Timing.VFreq, onTime: 25 * Timing.LineTime);
            HSync = new SquareSignal(frequency: Timing.HFreq, onTime: Timing.SyncTimes.LineSyncTime, onStartTime: Timing.SyncTimes.FrontPorchTime, amplitude: 0.3);
            VSync = new SquareSignal(frequency: 2.0 * Timing.HFreq, onTime: Timing.SyncTimes.VerticalSerrationTime, onStartTime: 0.5 * Timing.LineTime - Timing.SyncTimes.VerticalSerrationTime, amplitude: 0.3);
            var preEquOn = new SquareSignal(frequency: Timing.VFreq, onTime: 3 * Timing.LineTime);
            var postEquOn = new SquareSignal(frequency: Timing.VFreq, onTime: 3 * Timing.LineTime, onStartTime: 6 * Timing.LineTime);
            VEquOn = new AddSignal(preEquOn, postEquOn);
            VSyncOn = new SquareSignal(frequency: Timing.VFreq, onTime: 3 * Timing.LineTime, onStartTime: 3 * Timing.LineTime);
            VEqu = new SquareSignal(frequency: 2.0 * Timing.HFreq, onTime: (0.5 * Timing.LineTime) - Timing.SyncTimes.EquPulseTime, onStartTime: Timing.SyncTimes.EquPulseTime, amplitude: 0.3);
        }

        private double SimulatedTime = 0;

        double? SyncValue(double simulatedTime) {
            bool isHBlank = HBlank.Get(simulatedTime) == 1;
            bool isVBlank = VBlank.Get(SimulatedTime) == 1;

            if (isHBlank) {
                return 0.3 - HSync.Get(SimulatedTime);
            } else if (isVBlank) {
                bool isVSync = VSyncOn.Get(SimulatedTime) == 1;
                bool isVEqu = VEquOn.Get(SimulatedTime) == 1;
                return isVEqu ? VEqu.Get(SimulatedTime) : (isVSync ? VSync.Get(SimulatedTime) : 0.3);
            } else {
                return null;
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
