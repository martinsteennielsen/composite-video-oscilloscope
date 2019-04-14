using System;

namespace CompositeVideoOscilloscope {
    public class LayerSignal : IScreenContent {
        private readonly View View;

        public LayerSignal(TimingConstants timing) {
            View = new View(timing, 0, 20, -2, 2);
        }

        public double PixelValue(int x, int y) => Value(pos: View.Scale(x, y));

        private double Value((double time, double voltage) pos) {
            double s1 = Signal(pos.time) - pos.voltage - View.Scaler.dY;
            double s2 = Signal(pos.time) - pos.voltage + View.Scaler.dY;
            double s3 = Signal(pos.time - View.Scaler.dX) - pos.voltage;
            double s4 = Signal(pos.time + View.Scaler.dX) - pos.voltage;
            int d1 = s1 > 0 ? 1 : -1;
            int d2 = s2 > 0 ? 1 : -1;
            int d3 = s3 > 0 ? 1 : -1;
            int d4 = s4 > 0 ? 1 : -1;
            return Math.Abs(d1 + d2 + d3 + d4) == 4 ? 1 : 4;
        }

        double Signal(double t) => Math.Sin(t) - 0.5 * Math.Sin(t * 3);
    }
}
