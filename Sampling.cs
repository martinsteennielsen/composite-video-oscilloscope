using System;

namespace CompositeVideoOscilloscope {

    public class Sampling {
        const int ns = (int)1e9;
        readonly int SampleTimeNs;
        readonly int SampleTimeNsRoot;
        readonly int[] Buffer;
        readonly int[] SlopeBuffer;

        public Sampling(int[] buffer, double sampleTime) {
            Buffer = buffer;
            SampleTimeNsRoot = (int)Math.Sqrt(ns * sampleTime);
            var bufLen = buffer.Length;
            SlopeBuffer = new int[bufLen];
            for (int a = 0; a < bufLen - 1; a++) {
                SlopeBuffer[a] = (Buffer[a + 1] - Buffer[a])/SampleTimeNsRoot;
            }
            SlopeBuffer[bufLen - 1] = SlopeBuffer[bufLen - 2];
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

        public void ResetState(SamplingState current, (int t, int v) start, (int t, int v) delta, bool interpolate) {
            current.ScreenVoltage = start.v;
            current.DeltaScreenVoltage = delta.v;
            current.BufPos = start.t / SampleTimeNs;
            current.BufPosfraction = start.t % SampleTimeNs / SampleTimeNsRoot;
            current.DeltaBufPos = delta.t / SampleTimeNs;
            current.DeltaBufPosFraction = delta.t % SampleTimeNs / SampleTimeNsRoot;
            current.Interpolation = interpolate;
            SetCurrentValue(current);
        }

        public int GetNext(SamplingState current) {
            SetCurrentValue(current);

            current.ScreenVoltage += current.DeltaScreenVoltage;
            current.BufPos += current.DeltaBufPos;
            current.BufPosfraction += current.DeltaBufPosFraction;
            if (current.BufPosfraction <= 0) {
                current.BufPosfraction += SampleTimeNsRoot;
                current.BufPos--;
            } else if (current.BufPosfraction > SampleTimeNsRoot) {
                current.BufPosfraction -= SampleTimeNsRoot;
                current.BufPos++;
            }
            return current.Value;
        }

        private void SetCurrentValue(SamplingState current) {
            if (current.BufPos >= 0 && current.BufPos < Buffer.Length) {
                var interpolationDelta = current.Interpolation ? SlopeBuffer[current.BufPos] * current.BufPosfraction : 0;
                current.Value = current.ScreenVoltage  > interpolationDelta + Buffer[current.BufPos] ? 1 : -1;
            } else {
                current.Value = 8;
            }
        }


    }
}
