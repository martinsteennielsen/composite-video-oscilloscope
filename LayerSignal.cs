using System;

namespace CompositeVideoOscilloscope {
    public class LayerSignal : Layer, ILayer {
        (double dt, double dv) Scale;

        public LayerSignal(TimingConstants timing) : base(timing, 0, 20, -2, 2) {
            Scale = View.Scale();
        }

        public double PixelValue(int x, int y, double currentValue) => currentValue * Value(View.ToView(x, y));

        private double Value((double t, double v) pos) {
            double s1 = Signal(pos.t) - pos.v - Scale.dv;
            double s2 = Signal(pos.t) - pos.v + Scale.dv;
            double s3 = Signal(pos.t - Scale.dt) - pos.v;
            double s4 = Signal(pos.t + Scale.dt) - pos.v;
            int d1 = s1 > 0 ? 1 : -1;
            int d2 = s2 > 0 ? 1 : -1;
            int d3 = s3 > 0 ? 1 : -1;
            int d4 = s4 > 0 ? 1 : -1;
            if (Math.Abs(d1 + d2 + d3 + d4) == 4) {
                return 1;
            } else {
                return 4;
            }
        }

        double Signal(double t) => Math.Sin(t) - 0.5 * Math.Sin(t * 3);
    }
}
