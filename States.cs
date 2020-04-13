using System;

namespace CompositeVideoOscilloscope {

    public class LineState {
        public LineState(int noOfPlots) {
            PixelState = new PixelState(noOfPlots);
        }

        public bool Finished;

        public int LineBlockCount;
        public int LineCnt;
        public int LineNumber;
        public PixelState PixelState;
    }

    public class PixelState {
        public PixelState(int noOfPlots) {
            ContentState = new ContentState(noOfPlots);
        }

        public bool Finished;

        public long TimePs;
        public int LineSegmentCnt;
        public ContentState ContentState;
    }

    public class ContentState {
        public ContentState(int noOfPlots) {
            PlotStates = Array.ConvertAll(new PlotState[noOfPlots], v => new PlotState());
            PlotsVisible = new bool[noOfPlots];
        }

        public int LocationX, LocationY;
        public bool[] PlotsVisible;
        public PlotState[] PlotStates;
    }

    public class PlotState {
        public AxisLayerState AxisState = new AxisLayerState();
        public SignalLayerState SignalState = new SignalLayerState();
    }

    public class AxisLayerState {
        public (int X, int Y) Location = (0, 0);
    }

    public class SignalLayerState {
        public SignalLayerSamplingState SamplingState = new SignalLayerSamplingState();
        public SignalLayerSubSamplingState SubSamplingState = new SignalLayerSubSamplingState();
    }

    public class SignalLayerSamplingState {
        public int a, b, c, d;
        public SamplingState A = new SamplingState();
        public SamplingState B = new SamplingState();
        public SamplingState C = new SamplingState();
        public SamplingState D = new SamplingState();
    }

    public class SignalLayerSubSamplingState {
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
        public int BufPos;
        public int DeltaBufPos;
        public int BufPosfraction;
        public int DeltaBufPosFraction;
        public int ScreenVoltage;
        public int DeltaScreenVoltage;
        public int Value;
        public bool Interpolation;
    }
}
