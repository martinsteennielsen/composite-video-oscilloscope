using System;

namespace CompositeVideoOscilloscope {


    public class TriggerControls {
        public double Voltage;
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
        public (double Time, double Voltage) Offset;
        public (double Time, double Voltage) Units;
        public (double Time, double Voltage) Position;
        public TriggerControls Trigger;
        public int IntensityAxis = 0x00;
        public int IntensityBackground = 0x20;
        public int IntensitySignal = 0xFF;
        public SubSampleRender SubSamplePoints = SubSampleRender.ConnectLine;
    }
    

    public class Controls {
        public VideoStandard VideoStandard;
        public double CurrentTime;
        public int BytesGenerated;
        public PlotControls PlotControls;
        public double BitsPrSecond => CurrentTime == 0 ? 0  : BytesGenerated / CurrentTime * 1e-6;

    }
}