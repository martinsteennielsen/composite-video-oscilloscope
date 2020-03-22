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

        public void ResetState(SamplingState current, (int t, int v) start, (int t, int v) delta) {
            var deltaMod = delta.t % SampleTimeNs;
            var startFrac = Math.Abs(start.t % SampleTimeNs);

            current.BufPos = start.t / SampleTimeNs;
            current.DeltaBufPos = (delta.t / SampleTimeNs);
            current.DeltaDivisor = Math.Abs(deltaMod);
            current.DeltaDivisorOverrun = deltaMod > 0 ? 1 : -1;
            current.Divisor = deltaMod > 0 ? SampleTimeNs - startFrac : startFrac;
            current.DeltaScreenVoltage = delta.v;
            current.ScreenVoltage = start.v;
            current.SampleVoltage = 0;
            current.Value = 8;

            if (current.BufPos >= 0 && current.BufPos < Buffer.Length) {
                current.SampleVoltage = Buffer[current.BufPos];
                current.Value = current.ScreenVoltage > current.SampleVoltage ? 1 : -1;
            }
        }

        public int GetNext(SamplingState current) {
            var delta = current.DeltaBufPos;
            current.Divisor -= current.DeltaDivisor;
            if (current.Divisor <= 0) {
                current.Divisor += SampleTimeNs;
                delta += current.DeltaDivisorOverrun;
            }
            current.BufPos += delta;

            current.ScreenVoltage += current.DeltaScreenVoltage;
            if (current.BufPos >= 0 && current.BufPos < Buffer.Length) {
                current.SampleVoltage = Buffer[current.BufPos];
                current.Value = current.ScreenVoltage > current.SampleVoltage ? 1 : -1;
            } else {
                current.Value = 8;
            }
            return current.Value;
        }
    }
}
