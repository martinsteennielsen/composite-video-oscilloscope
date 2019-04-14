using System;


namespace CompositeVideoOscilloscope {


    public class LayerAxis : IScreenContent {
        readonly double GridPercentage;
        readonly View View;

        public LayerAxis(TimingConstants timing, double gridPercentage) {
            View = new View(timing, 0, 100, 0, 100);
            GridPercentage = gridPercentage;
        }

        public double PixelValue(int x, int y) => Value( View.Scale(x,y) );

        private double Value( (double x, double y) position) {
            if (Math.Abs(position.x % GridPercentage) < View.Scaler.dX) { return 0; }
            if (Math.Abs(position.y % GridPercentage) < View.Scaler.dY) { return 0; }
            return 1;
        }
    }
}
