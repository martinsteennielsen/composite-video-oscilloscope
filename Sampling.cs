using System;

namespace CompositeVideoOscilloscope {
    public class Sampling {
        public readonly double StartTimeNs;
        readonly double SampleDuration;
        readonly double SampleTimeNs;
        readonly double[] Buffer;
        public readonly double TriggerTimeNs;
        public readonly SubSampleRender SubSamplePoints;

        public Sampling(double[] buffer, double startTime, double endTime, double sampleTime, PlotControls controls) {
            Buffer = buffer;
            StartTimeNs = 1e9*startTime;
            SampleDuration = 1e9*(endTime - startTime);
            SampleTimeNs = 1e9*sampleTime;
            TriggerTimeNs = RunTrigger(controls.Trigger);
            SubSamplePoints = controls.SubSamplePoints;
        }

        double RunTrigger(TriggerControls trigger) {
            double timeNs = SampleTimeNs;
            for (int idx=1; idx<Buffer.Length; idx++) {
                double currentVoltage = Buffer[idx];
                double lastVoltage = Buffer[idx-1];
                bool currentTrigger = currentVoltage > trigger.Voltage;
                bool lastTrigger = lastVoltage > trigger.Voltage;

                if (currentTrigger && !lastTrigger && currentVoltage - lastVoltage > trigger.Edge) {
                    double interpolatedTime = InterpolateTime(lastVoltage, currentVoltage, trigger.Voltage);
                    if (double.IsInfinity(interpolatedTime)) { interpolatedTime = 0; }
                    return interpolatedTime + timeNs - SampleTimeNs;
                }
                timeNs += SampleTimeNs;
            }
            return 0;
        }

        private double InterpolateTime(double v0, double v1, double vt) =>
            SampleTimeNs * (vt - v0) / (v1 + v0);

        private double InterpolateVoltage(double v0, double  v1, double t) =>
            (1 - t) * v0 + t * v1;
        
        public bool TryGet(double timeOffsetNs, out double value) {
            if (timeOffsetNs < 0) {
                value = 0;
                return false;
            }
            if (timeOffsetNs + SampleTimeNs >= SampleDuration) {
                value = 0;
                return false;
            }
            double offset = (timeOffsetNs / SampleTimeNs) % 1.0;
            int bufpos = (int)(timeOffsetNs / SampleTimeNs);
            
            if (SubSamplePoints == SubSampleRender.ConnectStairs) {
                value = Buffer[bufpos];
            } else if (SubSamplePoints == SubSampleRender.ConnectLine ) {
                value = InterpolateVoltage(Buffer[bufpos], Buffer[bufpos + 1], offset);
            } else {
                value = Buffer[bufpos];
                return offset<0.1;
            }
            return true;
        }

    } 
}