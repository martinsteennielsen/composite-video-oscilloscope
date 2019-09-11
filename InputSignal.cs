using System;

namespace CompositeVideoOscilloscope {

    public class InputSignal {
        private readonly double[] Buffer;
        private  readonly double SampleTime;
        private double StartTime, EndTime;
        
        public double TriggerOffsetTime = 0;
        public InputSignal() {
            Buffer = new double[1000];
            SampleTime = .1/Buffer.Length;
        }

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

        public void Run(double currentTime, double triggerVoltage, double triggerEdge) {
            TriggerOffsetTime = 0;
            StartTime=currentTime;
            double time = StartTime;
            bool triggered = false;
            for (int idx=0; idx<Buffer.Length; idx++) {
                double currentVoltage = Generate(time);
                if (!triggered && idx > 0) {
                    double lastVoltage = Buffer[idx-1];
                    bool currentTrigger = currentVoltage > triggerVoltage;
                    bool lastTrigger = lastVoltage > triggerVoltage;

                    if (currentTrigger && !lastTrigger 
                        && currentVoltage - lastVoltage > triggerEdge ){
                        double interpolatedTime = InterpolateTime(lastVoltage, currentVoltage, triggerVoltage);
                        TriggerOffsetTime = interpolatedTime + time - (SampleTime + StartTime);
                        triggered=true;
                    }
                }
                Buffer[idx] = currentVoltage;
                time += SampleTime;
            }
            EndTime=time;
        }

        private double InterpolateTime(double v0, double v1, double vt) =>
            SampleTime * (vt - v0) / (v1 + v0);

        private double Generate(double t)=> Math.Sin(t*500) - 0.5 * Math.Sin(t * 0.5);
    }
}