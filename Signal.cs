using System;

namespace CompositeVideoOscilloscope {

    public interface ISignal {
        double Get(double time);
    }

    public class SquareSignal : ISignal {
        readonly double OnStartTime, OnTime, OffTime;

        public SquareSignal(double frequency, double onTime, double onStartTime = 0) { 
            OnStartTime = onStartTime;
            OnTime = onTime;
            OffTime = (1.0 / frequency) - onTime;
        }

        public double Get(double time) => ((OnTime + OffTime + time - OnStartTime) % (OnTime + OffTime) ) > OnTime ? 0 : 1;
    }
}
