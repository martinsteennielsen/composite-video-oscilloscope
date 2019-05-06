using System;

namespace CompositeVideoOscilloscope {

    public class InputSignal {
        private readonly double[] Buffer;
        private readonly double StartTime, EndTime, SampleTime;
        public InputSignal() {
            Buffer = new double[1000];
            StartTime=20; EndTime=40; SampleTime = (EndTime-StartTime)/Buffer.Length;
            
            for (int idx=0; idx<Buffer.Length; idx++) {
                double time = StartTime + idx*SampleTime;
                Buffer[idx] = Generate(time); 
            }
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

        private double Generate(double t)=> Math.Sin(t) - 0.5 * Math.Sin(t * 3);
    }
}