using System;

namespace CompositeVideoOscilloscope {


    public class LayerAxisI {
        public (double X, double Y) Current = (0, 0);

        public class LayerAxis {
            private readonly Viewport View;
            private readonly double Width;
            private readonly (double X, double Y) Delta;


            public LayerAxis(Viewport screen, double noOfDivisions, double angle) {
                View = screen.SetView(10, 10, 10 + noOfDivisions, 10 + noOfDivisions, angle);
                var (dX, dY) = View.Transform(1, 1);
                var (doX, doY) = View.Transform(0, 0);
                Width = Math.Sqrt((dX - doX) * (dX - doX) + (dY - doY) * (dY - doY));
                var (_dX, _dY) = View.Transform(1, 0);
                Delta = (_dX - doX, _dY - doY);
            }

            public void Next(LayerAxisI iter) {
                iter.Current.X += Delta.X;
                iter.Current.Y += Delta.Y;
            }

            public LayerAxisI Start(int x, int y) =>
                new LayerAxisI { Current = View.Transform(x, y) };

            public int Get(LayerAxisI iter) =>
                 Value(iter.Current);

            private int Value((double x, double y) pos) =>
                 pos.x % 1 < Width || pos.y % 1 < Width ? 0xFF : 0;
        }
    }
}