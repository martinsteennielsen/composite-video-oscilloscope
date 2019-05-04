using System;

namespace CompositeVideoOscilloscope {

    public class InputSignal {
        private readonly double[] Buffer;
        private readonly double StartTime, EndTime, SampleTime;
        public InputSignal() {
            Buffer = new double[1000];
            StartTime=10; EndTime=30; SampleTime = (EndTime-StartTime)/Buffer.Length;
            
            for (int idx=0; idx<Buffer.Length; idx++) {
                double time = StartTime + idx*SampleTime;
                Buffer[idx] = Generate(time); 
            }
        }
        public bool TryGet(double time, out double value) {
            value=0;
            if (time < StartTime) { 
                return false; 
            }
            if (time >= EndTime) { 
                return false;
            }
            int bufpos = (int)((time-StartTime)/SampleTime);
            value = Buffer[bufpos];
            return true;
        }
        private double Generate(double t)=> Math.Sin(t) - 0.5 * Math.Sin(t * 3);
    }
}