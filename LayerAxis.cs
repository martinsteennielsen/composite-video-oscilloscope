using System;

namespace CompositeVideoOscilloscope {

    public class LayerAxis : IScreenContent {
        readonly Viewport View;
        readonly double dX, dY;

        public LayerAxis(Viewport screen, double noOfDivisions, double angle) {
            View = screen.SetView(0,0, noOfDivisions, noOfDivisions, angle);
            dX = noOfDivisions / screen.Width;
            dY = noOfDivisions / screen.Height;
        }

        public int PixelValue(int x, int y) =>
             Value(View.Transform(x,y));

        private int Value((double x, double y) pos) =>
            (Frac(pos.x)>Frac(pos.x+dX)) || (Frac(pos.y)>Frac(pos.y+dY)) ? 0 :255;

        private double Frac(double x) => 
            x - Math.Truncate(x);
    }
}
