
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
        public int SampleBufferLength;
        public double NumberOfDivisions;
        public (double Time, double Voltage) Offset;
        public (double Time, double Voltage) Units;
        public (double Time, double Voltage) Position;
        public TriggerControls Trigger;
        public int IntensityAxis = 0x00;
        public int IntensityBackground = 0x20;
        public int IntensitySignal = 0xFF;
        public bool SubSamplePlot = false;
        public SubSampleRender SubSamplePoints = SubSampleRender.ConnectLine;
        public LocationControls Location;
    }


    public class Controls {
        public VideoStandard VideoStandard;
        public double CurrentTime;
        public PlotControls Plot1;
        public PlotControls Plot2;
        public bool RunMovements;
        public bool RunTime;
        public bool EnableOutput;
    }
}