using System;

namespace CompositeVideoOscilloscope {

    public struct Controls {
        public double NumberOfDivisions;
        public TimingConstants Timing;
        public double CurrentTime;
        public (double Time, double Voltage) Units;

        public Controls WithDivisions(int divisions) =>
            new Controls(this) { NumberOfDivisions = divisions };
        public Controls WithTiming(TimingConstants timing) =>
            new Controls(this) { Timing = timing };
        public Controls WithUnits(double timePrDivision, double voltagePrDivision) =>
            new Controls(this) { Units = (timePrDivision, voltagePrDivision) };
        public Controls WithTime(double time) =>
            new Controls(this) { CurrentTime = time };

        private Controls(Controls source) { this = source; }
    }
}