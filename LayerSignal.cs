using System;

namespace CompositeVideoOscilloscope {
    public class LayerSignal : IScreenContent {
        private readonly View View;
        double[,] Buffer;

        public LayerSignal(ScreenResolution resolution) {
            View = new View(0, 20, -2, 2, resolution: new ScreenResolution(resolution.Width, resolution.Height));
            Buffer = new double[3*resolution.Width, 3];
        }

        public double PixelValue(int x, int y) => Value(2*x,2*y, pos: View.Transform(x,y));

        private double Value(int x, int y, (double time, double voltage) pos) {
            ref double bp(int bx,int by) => ref Buffer[bx % Buffer.Length, by % 3];
            bp(x+2,y+2) = SubValue(pos);
            return ( bp(x,y) + bp(x+1,y) + bp(x+2,y) + bp(x, y+1) + bp(x+1, y+1) + bp(x+2, y+1) + bp(x, y+2) + bp(x+1, y+2) + bp(x+2, y+2) ) / 9;
        }

        private double SubValue((double time, double voltage) pos) {
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
