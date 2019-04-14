
using System;

namespace CompositeVideoOscilloscope {

    public interface ILayer {
        double PixelValue(int x, int y, double currentValue);
    }

    public abstract class Layer {
        protected readonly ViewPort View;

        public Layer(TimingConstants timing, double minX, double maxX, double minY, double maxY) {
            View = new ViewPort(timing, minX,maxX,minY,maxY);
        }

        public struct ViewPort {
            readonly double ViewMinX, ViewMaxX, ViewMinY, ViewMaxY, ViewWidth, ViewHeight;
            readonly double ScrWidth, ScrHeight, ScaleX, ScaleY;

            public ViewPort(TimingConstants timing, double minX, double maxX, double minY, double maxY) {
                ViewMinX = minX; ViewMaxX = maxX; ViewMinY = minY; ViewMaxY = maxY;
                ViewWidth = ViewMaxX - ViewMinX; ViewHeight = ViewMaxY - ViewMinY;
                ScrWidth = timing.BandwidthFreq / timing.HFreq;
                ScrHeight = 2d * timing.HFreq / timing.VFreq;
                ScaleX = ViewWidth / ScrWidth;
                ScaleY = ViewHeight / ScrHeight;
            }
            public (double, double) ToView(double x, double y) => ( x * ScaleX + ViewMinX, y * ScaleY + ViewMinY );
            public (double dx, double dy) Scale() => (ScaleX, ScaleY);
        }
    }
}
