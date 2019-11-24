using System;

namespace CompositeVideoOscilloscope {
    public class Sampling {
        public readonly double StartTime;
        readonly double EndTime;
        readonly double SampleTime;
        readonly double[] Buffer;
        public readonly double TriggerTime;

        public Sampling(double[] buffer, double startTime, double endTime, double sampleTime, TriggerControls trigger) {
            Buffer = buffer;
            StartTime = startTime;
            EndTime = endTime;
            SampleTime = sampleTime;
            TriggerTime = RunTrigger(trigger);
        }

        double RunTrigger(TriggerControls trigger) {
            double time = StartTime+SampleTime;
            for (int idx=1; idx<Buffer.Length; idx++) {
                double currentVoltage = Buffer[idx];
                double lastVoltage = Buffer[idx-1];
                bool currentTrigger = currentVoltage > trigger.Voltage;
                bool lastTrigger = lastVoltage > trigger.Voltage;

                if (currentTrigger && !lastTrigger && currentVoltage - lastVoltage > trigger.Edge) {
                    double interpolatedTime = InterpolateTime(lastVoltage, currentVoltage, trigger.Voltage);
                    if (double.IsInfinity(interpolatedTime)) { interpolatedTime = 0; }
                    return interpolatedTime + time - (SampleTime + StartTime);
                }
                time += SampleTime;
            }
            return 0;
        }

        private double InterpolateTime(double v0, double v1, double vt) =>
            SampleTime * (vt - v0) / (v1 + v0);

        private double InterpolateVoltage(double v0, double  v1, double t) =>
            (1 - t) * v0 + t * v1;
        
        public bool TryGet(double time, out double value) {
            if (time < StartTime) {
                value = 0;
                return false;
            }
            if (time + SampleTime >= EndTime) {
                value = 0;
                return false;
            }
            var timeDiff = time - StartTime;
            int bufpos = (int)(timeDiff / SampleTime);
            var offset = (timeDiff / SampleTime) % 1.0;
            value = InterpolateVoltage(Buffer[bufpos], Buffer[bufpos + 1], offset);
            return true;
        }

    } 
}