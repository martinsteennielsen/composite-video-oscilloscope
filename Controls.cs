using System;

namespace CompositeVideoOscilloscope {


    public class PlotControls {
        public double NumberOfDivisions;
        public (double Time, double Voltage) Offset;
        public (double Time, double Voltage) Units;
        public (double Time, double Voltage) Position;
        public double TriggerVoltage;
        public double TriggerEdge;
    }

    public class Controls {
        public VideoStandard VideoStandard;
        public double CurrentTime;
        public int IntensityAxis = 0x00;
        public int IntensityBackground = 0x20;
        public int IntensitySignal = 0xFF;
        public PlotControls PlotControls;

    }
}