using System;

namespace CompositeVideoOscilloscope {

    public class LayerAxis : IScreenContent {
        readonly Viewport View;
        readonly double Width;

        public LayerAxis(Viewport screen, double noOfDivisions, double angle) {
            View = screen.SetView(10,10, 10+noOfDivisions, 10+noOfDivisions, angle);
            var (dX,dY) = View.Transform(1,1);
            var (doX,doY) = View.Transform(0,0);
            Width = Math.Sqrt( (dX-doX)*(dX-doX) + (dY-doY)*(dY-doY));
        }

        public int PixelValue(int x, int y) =>
             Value(View.Transform(x,y));

        private int Value((double x, double y) pos) =>
             pos.x%1 < Width || pos.y%1 < Width ? 0 :255;
    }
}
