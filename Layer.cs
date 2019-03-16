
using System;

namespace CompositeVideoOscilloscope {
    public interface ILayer {
        double PixelValue(int x, int y, double currentValue);
    }

    public abstract class Layer: ILayer {
        readonly double ViewMinX, ViewMaxX, ViewMinY, ViewMaxY, ViewWidth, ViewHeight;
        readonly double ScrWidth, ScrHeight, ScaleX, ScaleY;

        public Layer(TimingConstants timing, double minX, double maxX, double minY, double maxY) {
            ViewMinX = minX; ViewMaxX = maxX; ViewMinY = minY; ViewMaxY = maxY;
            ViewWidth = ViewMaxX - ViewMinX; ViewHeight = ViewMaxY - ViewMinY;
            ScrWidth = timing.BandwidthFreq / timing.HFreq;
            ScrHeight = 2d * timing.HFreq / timing.VFreq;
            ScaleX = ViewWidth / ScrWidth;
            ScaleY = ViewHeight / ScrHeight;
        }

        public double PixelValue(int x, int y, double currentValue) {
            return currentValue * Value( (x * ScaleX) + ViewMinX, (y * ScaleY) + ViewMinY, ScaleX, ScaleY);
        }
        protected abstract double Value(double x, double y, double dx, double dy);
    }

    public class AxisLayer : Layer {
        public AxisLayer(TimingConstants timing) : base(timing, 0, 200, 0, 200) { }

        protected override double Value(double x, double y, double dx, double dy) {
            if (Math.Abs(x % 20) < dx) { return 0; }
            if (Math.Abs(y % 20) < dy) { return 0; }
            return 1;
        }
    }

    public class BackgroundLayer : Layer {
        public BackgroundLayer(TimingConstants timing) : base(timing, 0, 1, 0, 1) { }
        protected override double Value(double x, double y, double dx, double dy) => .2;
    }

    public class SignalLayer : Layer {
        public SignalLayer(TimingConstants timing) : base(timing, 0, 20, -2, 2) { }
        protected override double Value(double t, double v, double dt, double dv) {
            double s1 = Signal(t);
            double s2 = Signal(t + dt);
            int d1 = s1 - v > 0 ? 1 : -1;
            int d2 = s2 - v > 0 ? 1 : -1;
            int d3 = s1 - v + dv > 0 ? 1 : -1;
            int d4 = s2 - v + dv > 0 ? 1 : -1;
            return Math.Abs(d1 + d2 + d3 + d4) == 4 ? 1 : 10;
        }

        double Signal(double t) => Math.Sin(t) - 0.5 * Math.Sin(t*3);
    }

}
