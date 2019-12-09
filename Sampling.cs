using System;

namespace CompositeVideoOscilloscope {
    public class Sampling {
        public readonly double StartTime;
        readonly double SampleDuration;
        readonly double SampleTime;
        readonly double[] Buffer;
        public readonly double TriggerTime;
        public readonly SubSampleRender SubSamplePoints;

        public Sampling(double[] buffer, double startTime, double endTime, double sampleTime, PlotControls controls) {
            Buffer = buffer;
            StartTime = startTime;
            SampleDuration = endTime - StartTime;
            SampleTime = sampleTime;
            TriggerTime = RunTrigger(controls.Trigger);
            SubSamplePoints = controls.SubSamplePoints;
        }

        double RunTrigger(TriggerControls trigger) {
            double time = SampleTime;
            for (int idx=1; idx<Buffer.Length; idx++) {
                double currentVoltage = Buffer[idx];
                double lastVoltage = Buffer[idx-1];
                bool currentTrigger = currentVoltage > trigger.Voltage;
                bool lastTrigger = lastVoltage > trigger.Voltage;

                if (currentTrigger && !lastTrigger && currentVoltage - lastVoltage > trigger.Edge) {
                    double interpolatedTime = InterpolateTime(lastVoltage, currentVoltage, trigger.Voltage);
                    if (double.IsInfinity(interpolatedTime)) { interpolatedTime = 0; }
                    return interpolatedTime + time - SampleTime;
                }
                time += SampleTime;
            }
            return 0;
        }

        private double InterpolateTime(double v0, double v1, double vt) =>
            SampleTime * (vt - v0) / (v1 + v0);

        private double InterpolateVoltage(double v0, double  v1, double t) =>
            (1 - t) * v0 + t * v1;
        
        public bool TryGet(double timeOffset, out double value) {
            if (timeOffset < 0) {
                value = 0;
                return false;
            }
            if (timeOffset + SampleTime >= SampleDuration) {
                value = 0;
                return false;
            }
            double offset = (timeOffset / SampleTime) % 1.0;
            int bufpos = (int)(timeOffset / SampleTime);
            
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