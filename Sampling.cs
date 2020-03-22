using System;

namespace CompositeVideoOscilloscope {

    public class Sampling {
        const int ns = (int)1e9;
        readonly int SampleDuration;
        readonly int SampleTimeNs;
        readonly int[] Buffer;

        public Sampling(int[] buffer, double startTime, double endTime, double sampleTime, PlotControls controls) {
            Buffer = buffer;
            SampleDuration = (int)(ns * (endTime - startTime));
            SampleTimeNs = (int)(ns * sampleTime);
        }

        public int RunTrigger(TriggerControls trigger) {
            int timeNs = SampleTimeNs;
            int triggerVoltage = (int)(1e6 * trigger.Voltage);
            for (int idx = 1; idx < Buffer.Length; idx++) {
                var currentVoltage = Buffer[idx];
                var lastVoltage = Buffer[idx - 1];
                bool currentTrigger = currentVoltage > triggerVoltage;
                bool lastTrigger = lastVoltage > triggerVoltage;

                if (currentTrigger && !lastTrigger && currentVoltage - lastVoltage > trigger.Edge) {
                    var interpolatedTime = lastVoltage + currentVoltage == 0 ? 0 : InterpolateTime(lastVoltage, currentVoltage, triggerVoltage);
                    return interpolatedTime + timeNs - SampleTimeNs;
                }
                timeNs += SampleTimeNs;
            }
            return 0;
        }

        private int InterpolateTime(int v0, int v1, int vt) =>
            (((vt - v0) << 8) / (v1 + v0)) >> 8;

        public void Reset(SamplingIterator iter, (int t, int v) start, (int t, int v) delta) {
            var deltaMod = delta.t % SampleTimeNs;
            var startFrac = Math.Abs(start.t % SampleTimeNs);

            iter.BufPos = start.t / SampleTimeNs;
            iter.DeltaBufPos = (delta.t / SampleTimeNs);
            iter.DeltaDivisor = Math.Abs(deltaMod);
            iter.DeltaDivisorOverrun = deltaMod > 0 ? 1 : -1;
            iter.Divisor = deltaMod > 0 ? SampleTimeNs - startFrac : startFrac;
            iter.DeltaScreenVoltage = delta.v;
            iter.ScreenVoltage = start.v;
            iter.SampleVoltage = 0;
            iter.CurrentValue = 8;

            if (iter.BufPos >= 0 && iter.BufPos < Buffer.Length) {
                iter.SampleVoltage = Buffer[iter.BufPos];
                iter.CurrentValue = iter.ScreenVoltage > iter.SampleVoltage ? 1 : -1;
            }
        }

        public int GetNext(SamplingIterator iter) {

            var delta = iter.DeltaBufPos;
            iter.Divisor -= iter.DeltaDivisor;
            if (iter.Divisor <= 0) {
                iter.Divisor += SampleTimeNs;
                delta += iter.DeltaDivisorOverrun;
            }
            iter.BufPos += delta;

            iter.ScreenVoltage += iter.DeltaScreenVoltage;
            if (iter.BufPos >= 0 && iter.BufPos < Buffer.Length) {
                iter.SampleVoltage = Buffer[iter.BufPos];
                iter.CurrentValue = iter.ScreenVoltage > iter.SampleVoltage ? 1 : -1;
            } else {
                iter.CurrentValue = 8;
            }
            return iter.CurrentValue;
        }
    }
}
