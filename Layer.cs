
using System;

namespace CompositeVideoOscilloscope {
    public interface ILayer {
        double PixelValue(int x, int y, double currentValue);
    }

    public abstract class Layer: ILayer {
        readonly double ViewMinX, ViewMaxX, ViewMinY, ViewMaxY, ViewWidth, ViewHeight;
        readonly double ScrWidth, ScrHeight, ScaleX, ScaleY, OffX, OffY;

        public Layer(TimingConstants timing, double minX, double maxX, double minY, double maxY) {
            ViewMinX = minX; ViewMaxX = maxX; ViewMinY = minY; ViewMaxY = maxY;
            ViewWidth = ViewMaxX - ViewMinX; ViewHeight = ViewMaxY - ViewMinY;
            ScrWidth = timing.BandwidthFreq / timing.HFreq;
            ScrHeight = 2d * timing.HFreq / timing.VFreq;
            ScaleX = ViewWidth / ScrWidth;
            ScaleY = ViewHeight / ScrHeight;
            OffX = ViewMinX + ViewWidth / 2;
            OffY = ViewMinY + ViewHeight / 2;
        }

        public double PixelValue(int x, int y, double currentValue) {
            return currentValue * Value( (x * ScaleX) + ViewMinX, (y * ScaleY) + ViewMinY);
        }
        protected abstract double Value(double x, double y);
    }

    public class AxisLayer : Layer {
        public AxisLayer(TimingConstants timing) : base(timing, 0, 200, 0, 200) { }

        protected override double Value(double x, double y) {
            if (Math.Abs(x % 20) < 0.4) { return .8; }
            if (Math.Abs(y % 20) < 0.4) { return 0.1; }
            return 1;
        }
    }

    public class BackgroundLayer : Layer {
        public BackgroundLayer(TimingConstants timing) : base(timing, 0, 1, 0, 1) { }
        protected override double Value(double x, double y) => .2;
    }

    public class SignalLayer : Layer {
        public SignalLayer(TimingConstants timing) : base(timing, -100, 100, -100, 100) { }
        protected override double Value(double x, double y) => 0.5 + 0.5 * (y / 100) * (x / 100);
    }

}
