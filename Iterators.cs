using System;
using System.Collections.Generic;
using System.Text;

namespace CompositeVideoOscilloscope {
    public class LineIterator {
        public bool Finished;

        public int LineBlockCount;
        public int LineCnt;
        public int LineNumber;
        public PixelIterator CurrentPixel;
    }

    public class PixelIterator {
        public bool Finished;

        public long CurrentTimePs;
        public int LineSegmentCnt;
        public ContentIterator CurrentContent;
    }

    public class ContentIterator {
        public int CurrentX, CurrentY;
        public PlotIterator Plot1, Plot2;
    }

    public class PlotIterator {
        public AxisPlotIterator Axis;
        public SignalPlotIterator Signal;
    }
    public class AxisPlotIterator {
        public (double X, double Y) Current = (0, 0);
    }

    public class SignalPlotIterator {
        public int a, b, c, d, e, f, g, h, i, j, k, l, m, n, o, p;
        public SamplingIterator iterB, iterF, iterG, iterI, iterM, iterN, iterP;
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
