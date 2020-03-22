using System;
using System.Collections.Generic;
using System.Text;

namespace CompositeVideoOscilloscope {
    public class LineIterator {
        public bool Finished;

        public int LineBlockCount;
        public int LineCnt;
        public int LineNumber;
        public PixelIterator CurrentPixel = new PixelIterator();
    }

    public class PixelIterator {
        public bool Finished;

        public long CurrentTimePs;
        public int LineSegmentCnt;
        public ContentIterator CurrentContent = new ContentIterator();
    }

    public class ContentIterator {
        public int CurrentX, CurrentY;
        public PlotIterator Current1, Current2;
        public PlotIterator Plot1 = new PlotIterator();
        public PlotIterator Plot2 = new PlotIterator();
    }

    public class PlotIterator {
        public AxisPlotIterator Axis = new AxisPlotIterator();
        public SignalPlotIterator Signal = new SignalPlotIterator();
    }

    public class AxisPlotIterator {
        public (double X, double Y) Current = (0, 0);
    }

    public class SignalPlotIterator {
        public int a, b, c, d, e, f, g, h, i, j, k, l, m, n, o, p;
        public SamplingIterator iterB = new SamplingIterator();
        public SamplingIterator iterF = new SamplingIterator();
        public SamplingIterator iterG = new SamplingIterator();
        public SamplingIterator iterI = new SamplingIterator();
        public SamplingIterator iterM = new SamplingIterator();
        public SamplingIterator iterN = new SamplingIterator();
        public SamplingIterator iterP = new SamplingIterator();
    }

    public class SamplingIterator {
        public int DeltaBufPos;
        public int DeltaDivisorOverrun;
        public int Divisor;
        public int DeltaDivisor;
        public int DeltaScreenVoltage;
        public int BufPos;
        public int ScreenVoltage;
        public int SampleVoltage;
        public int CurrentValue;
    }
}
