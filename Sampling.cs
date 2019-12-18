using System;

namespace CompositeVideoOscilloscope {
    public class Sampling {
        const int ns = (int)1e9;
        readonly int SampleDuration;
        readonly int SampleTimeNs;
        readonly int[] Buffer;
        public readonly SubSampleRender SubSamplePoints;

        public Sampling(int[] buffer, double startTime, double endTime, double sampleTime, PlotControls controls) {
            Buffer = buffer;
            SampleDuration = (int)(ns*(endTime - startTime));
            SampleTimeNs = (int)(ns*sampleTime);
            SubSamplePoints = controls.SubSamplePoints;
        }

        public int RunTrigger(TriggerControls trigger) {
            int timeNs = SampleTimeNs;
            int triggerVoltage = (int)(1e6 * trigger.Voltage);
            for (int idx=1; idx<Buffer.Length; idx++) {
                var currentVoltage = Buffer[idx];
                var lastVoltage = Buffer[idx-1];
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
            (((vt - v0)<<8) / (v1 + v0)) >>8;

        public Iteration StartIteration((int t, int v) start, (int t, int v) delta) {
            Iteration iter = new Iteration {
                CurBufPos = start.t / SampleTimeNs,
                CurBufPosFraction = start.t % SampleTimeNs,
                DeltaBufPos = (delta.t / SampleTimeNs),
                DeltaBufPosFraction = delta.t % SampleTimeNs,
                DeltaScreenVoltage = delta.v,
                CurScreenVoltage = start.v,
                CurSampleVoltage = 0,
                CurValue = 8
            };
            
            if (iter.CurBufPos >= 0 && iter.CurBufPos < Buffer.Length) {
                iter.CurSampleVoltage = Buffer[iter.CurBufPos];
                iter.CurValue = iter.CurScreenVoltage > iter.CurSampleVoltage ? 1 : -1;
            }
            return iter;
        }

        public int GetNext(Iteration iter) =>
            iter.DeltaBufPosFraction > 0 ? NextRight(iter) : NextLeft(iter);

        int NextRight(Iteration iter) {
            iter.CurScreenVoltage += iter.DeltaScreenVoltage;
            var delta = iter.DeltaBufPos;
            iter.CurBufPosFraction += iter.DeltaBufPosFraction;
            if (iter.CurBufPosFraction >= SampleTimeNs) {
                delta++;
                iter.CurBufPosFraction -= SampleTimeNs;
            }
            if (delta != 0) {
                iter.CurBufPos += delta;
                if (iter.CurBufPos >= 0 && iter.CurBufPos < Buffer.Length) {
                    iter.CurSampleVoltage = Buffer[iter.CurBufPos];
                    iter.CurValue = iter.CurScreenVoltage > iter.CurSampleVoltage ? 1 : -1;
                } else {
                    iter.CurValue = 8;
                }
            } else {
                iter.CurValue = iter.CurScreenVoltage > iter.CurSampleVoltage ? 1 : -1;
            }
            return iter.CurValue;
        }

        int NextLeft(Iteration iter) {
            iter.CurScreenVoltage += iter.DeltaScreenVoltage;
            var delta = iter.DeltaBufPos;
            iter.CurBufPosFraction += iter.DeltaBufPosFraction;
            if (iter.CurBufPosFraction <= 0) {
                delta--;
                iter.CurBufPosFraction += SampleTimeNs;
            }
            if (delta != 0) {
                iter.CurBufPos += delta;
                if (iter.CurBufPos >= 0 && iter.CurBufPos < Buffer.Length) {
                    iter.CurSampleVoltage = Buffer[iter.CurBufPos];
                    iter.CurValue = iter.CurScreenVoltage > iter.CurSampleVoltage ? 1 : -1;
                } else {
                    iter.CurValue = 8;
                }
            } else {
                iter.CurValue = iter.CurScreenVoltage > iter.CurSampleVoltage ? 1 : -1;
            }
            return iter.CurValue;
        }

        public class Iteration {
            public int DeltaBufPos;
            public int DeltaBufPosFraction;
            public int DeltaScreenVoltage;
            public int CurBufPosFraction;
            public int CurBufPos;
            public int CurScreenVoltage;
            public int CurSampleVoltage;
            public int CurValue;
        }
    }
}