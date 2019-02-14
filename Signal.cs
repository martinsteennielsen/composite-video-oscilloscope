using System;

namespace CompositeVideoOscilloscope {

    public interface ISignal {
        double Get(double time);
    }

    public class AddSignal : ISignal {
        readonly ISignal Signal1, Signal2;
        public AddSignal(ISignal signal1, ISignal signal2) {
            Signal1 = signal1;
            Signal2 = signal2;
        }

        public double Get(double time) => Signal1.Get(time) + Signal2.Get(time);
    }

    public class SquareSignal : ISignal {
        readonly double OnStartTime, OnTime, OffTime, Amplitude;

        public SquareSignal(double frequency, double onTime, double onStartTime = 0, double amplitude = 1.0) {
            OnStartTime = onStartTime;
            OnTime = onTime;
            Amplitude = amplitude;
            OffTime = (1.0 / frequency) - onTime;
        }

        public double Get(double time) => ((OnTime + OffTime + time - OnStartTime) % (OnTime + OffTime)) > OnTime ? 0 : Amplitude;
    }
}
