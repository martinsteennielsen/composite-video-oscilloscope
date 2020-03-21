using System;
using static CompositeVideoOscilloscope.SamplingI;

namespace CompositeVideoOscilloscope {


    public class Aquisition {
        const int uV = (int)1e6; 
        private readonly int[] Buffer1;
        private readonly int[] Buffer2;
        private readonly double SampleTime;
        
        public Aquisition() {
            Buffer1 = new int[1000];
            Buffer2 = new int[1000];
            SampleTime = .1/Buffer1.Length;
        }

        public Sampling GetSampling(int channel, double currentTime, PlotControls controls) {
            if (channel == 1) {
                var (startTime, endTime) = Run(currentTime, Buffer1, Generate1);
                return new Sampling(Buffer1, startTime, endTime, SampleTime, controls);
            } else {
                var (startTime, endTime) = Run(currentTime, Buffer2, Generate2);
                return new Sampling(Buffer2, startTime, endTime, SampleTime, controls);
            }
        }
        
        (double, double) Run(double currentTime, int[] buffer, Func<double, double> generate) {
            var startTime=currentTime;
            double endTime = startTime;
            for (int idx=0; idx<buffer.Length; idx++) {
                buffer[idx] = (int)(uV*generate(endTime));
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