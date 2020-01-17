using System;

namespace CompositeVideoOscilloscope {

    public class LayerAxis  {
        private readonly Viewport View;
        private readonly double Width;
        private readonly (double X, double Y) Delta;

        private (double X, double Y) Current = (0,0);

        public LayerAxis(Viewport screen, double noOfDivisions, double angle) {
            View = screen.SetView(10,10, 10+noOfDivisions, 10+noOfDivisions, angle);
            var (dX,dY) = View.Transform(1,1);
            var (doX,doY) = View.Transform(0,0);
            Width = Math.Sqrt( (dX-doX)*(dX-doX) + (dY-doY)*(dY-doY));
            var (_dX, _dY) = View.Transform(1, 0);
            Delta = (_dX - doX, _dY - doY);
        }

        public void Next() {
            Current.X += Delta.X;
            Current.Y += Delta.Y;
        }

        public void Start(int x, int y) =>
            Current = View.Transform(x, y);

        public int Get() =>
             Value(Current);

        private int Value((double x, double y) pos) =>
             pos.x%1 < Width || pos.y%1 < Width ? 0xFF : 0;
    }
}
