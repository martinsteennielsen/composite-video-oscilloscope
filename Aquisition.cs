using System;

namespace CompositeVideoOscilloscope {

    public class Aquisition {
        private readonly double[] Buffer1;
        private readonly double[] Buffer2;
        private readonly double SampleTime;
        
        public Aquisition() {
            Buffer1 = new double[1000];
            Buffer2 = new double[1000];
            SampleTime = .1/Buffer1.Length;
        }

        public Sampling GetSampling(int channel, double currentTime, TriggerControls trigger) {
            if (channel == 1) {
                var (startTime, endTime) = Run(currentTime, Buffer1, Generate1);
                return new Sampling(Buffer1, startTime, endTime, SampleTime, trigger);
            } else {
                var (startTime, endTime) = Run(currentTime, Buffer2, Generate2);
                return new Sampling(Buffer2, startTime, endTime, SampleTime, trigger);
            }
        }
        
        (double, double) Run(double currentTime, double[] buffer, Func<double, double> generate) {
            var startTime=currentTime;
            double endTime = startTime;
            for (int idx=0; idx<buffer.Length; idx++) {
                buffer[idx] = generate(endTime);
                endTime += SampleTime;
            }
            return (startTime, endTime);
        }

        private double Generate1(double t) =>
            Math.Sin(t*500) - 0.5 * Math.Sin(t * 0.5);

        private double Generate2(double t) =>
            Math.Sign(Generate1(t));
    }
}