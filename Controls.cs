using System;

namespace CompositeVideoOscilloscope {

    public class Controls {
        public double NumberOfDivisions;
        public double CurrentTime;
        public (double Time, double Voltage) Offset;
        public (double left, double top, double right, double bottom) ScreenPosition;
        public VideoStandard VideoStandard;
        public (double Time, double Voltage) Units;
        public (double Time, double Voltage) Position;
        public double TriggerVoltage;
        public double TriggerEdge;
        public double Angle;
        public int IntensityAxis = 0x00;
        public int IntensityBackground = 0x20;
        public int IntensitySignal = 0xFF;

    }
}