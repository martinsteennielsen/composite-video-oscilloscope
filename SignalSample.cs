namespace CompositeVideoOscilloscope {
    public class SignalSample {
        readonly double StartTime;
        readonly double EndTime;
        readonly double SampleTime;
        readonly double[] Buffer;
        public readonly double TriggerOffsetTime;

        public SignalSample(double[] buffer, double startTime, double endTime, double sampleTime, double triggerVoltage, double triggerEdge) {
            Buffer = buffer;
            StartTime = startTime;
            EndTime = endTime;
            SampleTime = sampleTime;
            TriggerOffsetTime = GetTriggerOffset(buffer, startTime, sampleTime, triggerVoltage, triggerEdge);
        }

        double GetTriggerOffset(double[] buffer, double startTime, double sampleTime, double triggerVoltage, double triggerEdge) {
            double time = startTime+SampleTime;
            for (int idx=1; idx<Buffer.Length; idx++) {
                double currentVoltage = Buffer[idx];
                double lastVoltage = Buffer[idx-1];
                bool currentTrigger = currentVoltage > triggerVoltage;
                bool lastTrigger = lastVoltage > triggerVoltage;

                if (currentTrigger && !lastTrigger && currentVoltage - lastVoltage > triggerEdge) {
                    double interpolatedTime = InterpolateTime(lastVoltage, currentVoltage, triggerVoltage);
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