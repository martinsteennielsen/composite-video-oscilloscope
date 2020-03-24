namespace CompositeVideoOscilloscope {

    public class LineState {
        public bool Finished;

        public int LineBlockCount;
        public int LineCnt;
        public int LineNumber;
        public PixelState PixelState = new PixelState();
    }

    public class PixelState {
        public bool Finished;

        public long TimePs;
        public int LineSegmentCnt;
        public ContentState ContentState = new ContentState();
    }

    public class ContentState {
        public int LocationX, LocationY;
        public bool Plot1Visible, Plot2Visible;
        public PlotState Plot1State = new PlotState();
        public PlotState Plot2State = new PlotState();
    }

    public class PlotState {
        public AxisPlotState AxisState = new AxisPlotState();
        public SignalPlotState SignalState = new SignalPlotState();
    }

    public class AxisPlotState {
        public (double X, double Y) Location = (0, 0);
    }

    public class SignalPlotState {
        public PlotSamplingState SamplingState = new PlotSamplingState();
        public PlotSubSamplingState SubSamplingState = new PlotSubSamplingState();
    }

    public class PlotSamplingState {
        public int a, b, c, d;
        public SamplingState A = new SamplingState();
        public SamplingState B = new SamplingState();
        public SamplingState C = new SamplingState();
        public SamplingState D = new SamplingState();
    }

    public class PlotSubSamplingState {
        public int a, b, c, d, e, f, g, h, i, j, k, l, m, n, o, p;
        public SamplingState B = new SamplingState();
        public SamplingState F = new SamplingState();
        public SamplingState G = new SamplingState();
        public SamplingState I = new SamplingState();
        public SamplingState M = new SamplingState();
        public SamplingState N = new SamplingState();
        public SamplingState P = new SamplingState();
    }

    public class SamplingState {
        public int DeltaBufPos;
        public int DeltaDivisorOverrun;
        public int Divisor;
        public int DeltaDivisor;
        public int DeltaScreenVoltage;
        public int BufPos;
        public int ScreenVoltage;
        public int SampleVoltage;
        public int Value;
    }
}
