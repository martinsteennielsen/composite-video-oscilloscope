using System;

namespace CompositeVideoOscilloscope {


    public class TriggerControls {
        public int MikroVolt;
        public double Edge;
    }

    public class LocationControls {
        public double Left;
        public double Top;
        public double Right;
        public double Bottom;
        public double Angle;
    }

    public enum SubSampleRender {
        ConnectStairs = 0,
        ConnectLine = 1,
        NotConnected = 2,
    }

    public class PlotControls {
        public double NumberOfDivisions;
        public (double Time, double MicroVolt) Offset;
        public (double TimeNs, double MicroVolt) Units;
        public (double TimeNs, double MicroVolt) Position;
        public TriggerControls Trigger;
        public int IntensityAxis = 0x00;
        public int IntensityBackground = 0x20;
        public int IntensitySignal = 0xFF;
        public SubSampleRender SubSamplePoints = SubSampleRender.ConnectLine;
    }
    

    public class Controls {
        public VideoStandard VideoStandard;
        public double CurrentTime;
        public PlotControls PlotControls;

    }
}