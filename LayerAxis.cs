using System;


namespace CompositeVideoOscilloscope {


    public class LayerAxis : IScreenContent {
        readonly View View;
        readonly double GridPercentage;

        public LayerAxis(ScreenResolution resolution, double gridPercentage) {
            View = new View(0, 100, 0, 100, resolution);
            GridPercentage = gridPercentage;
        }

        public int PixelValue(int x, int y) => Value( View.Transform(x,y) );

        private int  Value( (double x, double y) position) {
            if (Math.Abs(position.x % GridPercentage) < View.Scaler.dX) { return 0; }
            if (Math.Abs(position.y % GridPercentage) < View.Scaler.dY) { return 0; }
            return 255;
        }
    }
}
