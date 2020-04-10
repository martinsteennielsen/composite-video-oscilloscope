using System;

namespace CompositeVideoOscilloscope {

    public class Sampling {
        const int ns = (int)1e9;
        readonly int SampleTimeNs;
        readonly int[] Buffer;
        readonly int[] SlopeBuffer;

        public Sampling(int[] buffer, double sampleTime) {
            Buffer = buffer;
            SlopeBuffer = new int[buffer.Length];
            for (int a = 0; a < buffer.Length - 2; a++) {
                SlopeBuffer[a] = Buffer[a + 1] - Buffer[a];
            }
            SlopeBuffer[buffer.Length - 1] = SlopeBuffer[buffer.Length - 2];
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
            current.DeltaBufPosDivisor = Math.Abs(deltaMod);
            current.DeltaBufPosDivisorOverrun = deltaMod > 0 ? 1 : -1;
            current.BufPosDivisor = deltaMod > 0 ? SampleTimeNs - startFrac : startFrac;
            current.DeltaScreenVoltage = delta.v;
            current.ScreenVoltage = start.v;
            current.SubSamplePosDivisor = current.DeltaBufPosDivisorOverrun == 1 ?
                current.BufPosDivisor : SampleTimeNs - current.BufPosDivisor;
            SetCurrentValue(current);
        }

        public int GetNext(SamplingState current) {
            SetCurrentValue(current);

            current.ScreenVoltage += current.DeltaScreenVoltage;
            current.BufPos += current.DeltaBufPos;
            current.BufPosDivisor -= current.DeltaBufPosDivisor;
            if (current.BufPosDivisor <= 0) {
                current.BufPosDivisor += SampleTimeNs;
                current.BufPos += current.DeltaBufPosDivisorOverrun;
            }
            current.SubSamplePosDivisor = current.DeltaBufPosDivisorOverrun == 1 ?
                SampleTimeNs - current.BufPosDivisor : current.BufPosDivisor;

            return current.Value;
        }

        private void SetCurrentValue(SamplingState current) {
            if (current.BufPos >= 0 && current.BufPos < Buffer.Length) {
                var delta = SlopeBuffer[current.BufPos] * current.SubSamplePosDivisor / SampleTimeNs;
                current.Value = current.ScreenVoltage  > delta + Buffer[current.BufPos] ? 1 : -1;
            } else {
                current.Value = 8;
            }
        }


    }
}
