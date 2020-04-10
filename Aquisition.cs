using System;
using static CompositeVideoOscilloscope.SamplingState;

namespace CompositeVideoOscilloscope {


    public class Aquisition {
        const int uV = (int)1e6; 

        public Sampling GetSampling(PlotControls controls, int channel, double currentTime) {
            if (channel == 1) {
                var buffer = new int[controls.SampleBufferLength];
                var sampleTime = .1 / buffer.Length;
                var (startTime, endTime) = Run(currentTime, buffer, sampleTime, Generate1);
                return new Sampling(buffer, .1 / buffer.Length);
            } else {
                var buffer = new int[controls.SampleBufferLength];
                var sampleTime = .1 / buffer.Length;
                var (startTime, endTime) = Run(currentTime, buffer, sampleTime, Generate2);
                return new Sampling(buffer, .1 / buffer.Length);
            }
        }
        
        (double, double) Run(double currentTime, int[] buffer, double sampleTime, Func<double, double> generate) {
            var startTime=currentTime;
            double endTime = startTime;
            for (int idx=0; idx<buffer.Length; idx++) {
                buffer[idx] = (int)(uV*generate(endTime));
                endTime += sampleTime;
            }
            return (startTime, endTime);
        }

        private double Generate1(double t) =>
            Math.Sin(t*500) - 0.5 * Math.Sin(t * 0.5);

        private double Generate2(double t) =>
            Math.Sign(Generate1(t));
    }
}