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

        double[] Values = new double[] {0,0,0,0,0};

        public bool TryGet(double time, double dt, out double[] values) {
            values=Values;
            if (time - 2*dt < StartTime) { 
                return false; 
            }
            if (time + 2*dt >= EndTime) { 
                return false;
            }
            int bufpos = (int)((time-StartTime-2*dt)/SampleTime);
            values[0] = Buffer[bufpos];
            bufpos = (int)((time-StartTime-dt)/SampleTime);
            values[1] = Buffer[bufpos];
            bufpos = (int)((time-StartTime)/SampleTime);
            values[2] = Buffer[bufpos];
            bufpos = (int)((time-StartTime + dt)/SampleTime);
            values[3] = Buffer[bufpos];
            bufpos = (int)((time-StartTime + 2*dt)/SampleTime);
            values[4] = Buffer[bufpos];
            return true;
        }

        private double Generate(double t)=> Math.Sin(t) - 0.5 * Math.Sin(t * 3);
    }
}