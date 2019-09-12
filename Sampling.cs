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
            TriggerTime = RunTrigger(buffer, startTime, sampleTime, trigger);
        }

        double RunTrigger(double[] buffer, double startTime, double sampleTime, TriggerControls trigger) {
            double time = startTime+SampleTime;
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

        public bool TryGet(double time, out double value) {
            if (time < StartTime) {
                value = 0;
                return false;
            }
            if (time >= EndTime) {
                value = 0;
                return false;
            }
            int bufpos = (int)((time - StartTime) / SampleTime);
            value = Buffer[bufpos];
            return true;
        }

    } 
}