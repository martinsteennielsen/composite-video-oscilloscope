using System;

namespace CompositeVideoOscilloscope {

    public struct Controls {
        public double NumberOfDivisions;
        public double CurrentTime, ElapsedTime;
        public (double Time, double Voltage) Offset;
        public (double left, double top, double right, double bottom) ScreenPosition;
        public VideoStandard VideoStandard;
        public (double Time, double Voltage) Units;
        public (double Time, double Voltage) Position;
        public double TriggerVoltage;
    }
}