using System;

namespace CompositeVideoOscilloscope {
    public class Sampling {
        const int ns = (int)1e9;
        readonly int SampleDuration;
        readonly int SampleTimeNs;
        readonly int[] Buffer;
        public readonly int TriggerTimeNs;
        public readonly SubSampleRender SubSamplePoints;

        public Sampling(int[] buffer, double startTime, double endTime, double sampleTime, PlotControls controls) {
            Buffer = buffer;
            SampleDuration = (int)(ns*(endTime - startTime));
            SampleTimeNs = (int)(ns*sampleTime);
            TriggerTimeNs = RunTrigger(controls.Trigger);
            SubSamplePoints = controls.SubSamplePoints;
        }

        int RunTrigger(TriggerControls trigger) {
            int timeNs = SampleTimeNs;
            for (int idx=1; idx<Buffer.Length; idx++) {
                var currentVoltage = Buffer[idx];
                var lastVoltage = Buffer[idx-1];
                bool currentTrigger = currentVoltage > trigger.MikroVolt;
                bool lastTrigger = lastVoltage > trigger.MikroVolt;

                if (currentTrigger && !lastTrigger && currentVoltage - lastVoltage > trigger.Edge) {
                    var interpolatedTime = lastVoltage + currentVoltage == 0 ? 0 : InterpolateTime(lastVoltage, currentVoltage, trigger.MikroVolt);
                    return interpolatedTime + timeNs - SampleTimeNs;
                }
                timeNs += SampleTimeNs;
            }
            return 0;
        }

        private int InterpolateTime(int v0, int v1, int vt) =>
            (SampleTimeNs*vt - SampleTimeNs*v0) / (v1 + v0);

        private int InterpolateVoltage(int v0, int v1, int t) =>
            ((1000 - t) * v0 + t * v1)/1000;
        
        public bool TryGet(int timeOffsetNs, out int value) {
            if (timeOffsetNs < 0) {
                value = 0;
                return false;
            }
            if (timeOffsetNs + SampleTimeNs >= SampleDuration) {
                value = 0;
                return false;
            }
            int offset = (timeOffsetNs / SampleTimeNs) % (int)1e3;
            int bufpos = (timeOffsetNs / SampleTimeNs);
            
            if (SubSamplePoints == SubSampleRender.ConnectStairs) {
                value = Buffer[bufpos];
            } else if (SubSamplePoints == SubSampleRender.ConnectLine ) {
                value = InterpolateVoltage(Buffer[bufpos], Buffer[bufpos + 1], offset);
            } else {
                value = Buffer[bufpos];
                return offset<100;
            }
            return true;
        }
    }
}