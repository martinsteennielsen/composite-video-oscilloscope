using System;

namespace CompositeVideoOscilloscope {
    public class LayerSignal : IScreenContent {
        private readonly View View;

        public LayerSignal(ScreenResolution resolution) {
            View = new View(0, 20, -2, 2, resolution: new ScreenResolution(2*resolution.Width, 2*resolution.Height));
        }

        public double PixelValue(int x, int y) => Value(2*x,2*y);

        private double Value(int x, int y) {
            (double time, double voltage) = View.Transform(x, y);

            var p1 = SubValue(time - View.Scaler.dX, voltage - View.Scaler.dY);
            var p2 = SubValue(time + View.Scaler.dX, voltage - View.Scaler.dY);
            var p3 = SubValue(time - View.Scaler.dX, voltage + View.Scaler.dY);
            var p4 = SubValue(time + View.Scaler.dX, voltage + View.Scaler.dY);
            var p5 = SubValue(time, voltage) << 2;

            return (double)  ( p1 + p2 + p3 + p4 + p5 ) / 8;
        }

        private int SubValue(double time, double voltage) {
            double s1 = Signal(time) - voltage - View.Scaler.dY;
            double s2 = Signal(time) - voltage + View.Scaler.dY;
            double s3 = Signal(time - View.Scaler.dX) - voltage;
            double s4 = Signal(time + View.Scaler.dX) - voltage;
            int d1 = s1 > 0 ? 1 : -1;
            int d2 = s2 > 0 ? 1 : -1;
            int d3 = s3 > 0 ? 1 : -1;
            int d4 = s4 > 0 ? 1 : -1;
            return Math.Abs(d1 + d2 + d3 + d4) == 4 ? 1 : 4;
        }


        // Define function PlotAntiAliasedPoint(number x, number y)
        // For roundedx = floor(x) to ceil(x ) do
        //    For roundedy = floor(y) to ceil(y ) do
        //      percent_x = 1 - abs(x - roundedx )
        //      percent_y = 1 - abs(y - roundedy )
        //      percent = percent_x* percent_y
        //      DrawPixel(coordinates roundedx, roundedy, color percent (range 0-1) )

        
        double Signal(double t) => Math.Sin(t) - 0.5 * Math.Sin(t * 3);
    }
}
