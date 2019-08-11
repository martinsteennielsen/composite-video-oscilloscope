using System;

namespace CompositeVideoOscilloscope {

    public struct Controls {
        public double NumberOfDivisions;
        public VideoStandard VideoStandard;
        public double CurrentTime, ElapsedTime;
        public (double Time, double Voltage) Units;
        public (double Time, double Voltage) Position;
        public (double Time, double Voltage) Offset;
        public double TriggerVoltage;

        public Controls WithDivisions(int divisions) =>
            new Controls(this) { NumberOfDivisions = divisions };

        public Controls WithTriggerLevel(double voltage) =>
            new Controls(this) { TriggerVoltage = voltage };

        public Controls WithVideoStandard(VideoStandard videoStandard) =>
            new Controls(this) { VideoStandard = videoStandard };

        public Controls WithUnits(double timePrDivision, double voltagePrDivision) =>
            new Controls(this) { Units = (timePrDivision, voltagePrDivision) };
        public Controls WithTime(double time) =>
            new Controls(this) { ElapsedTime = time - CurrentTime, CurrentTime = time,  };

        private Controls(Controls source) { this = source; }
    }
}