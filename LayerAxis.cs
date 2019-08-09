using System;


namespace CompositeVideoOscilloscope {

    public class LayerAxis : IScreenContent {
        readonly Viewport View;
        readonly double NoOfDivisions;
        readonly double dX, dY;

        public LayerAxis(Viewport screen, double noOfDivisions) {
            View = screen.SetView(0,0, noOfDivisions, noOfDivisions);
            dX = (View.Transform(1,0).x - View.Transform(0,0).x);
            dY = (View.Transform(0,1).y - View.Transform(0,0).y);
        }

        public int PixelValue(int x, int y) =>
             Value(View.Transform(x,y));

        private int Value((double x, double y) pos) =>
            (Frac(pos.x)>Frac(pos.x+dX)) || (Frac(pos.y)>Frac(pos.y+dY)) ? 0 :255;

        private double Frac(double x) => 
            x - Math.Truncate(x);
        
        public void VSync() { }
    }
}
