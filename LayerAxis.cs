using System;

namespace CompositeVideoOscilloscope {

    public class LayerAxis {
        private const int Scaler = 10000;
        private readonly Viewport View;
        private readonly int Width;
        private readonly (int X, int Y) Delta;

        public LayerAxis(Viewport screen, double noOfDivisions, double angle) {
            View = screen.SetView(Scaler*10, Scaler*10, Scaler*(10 + noOfDivisions), Scaler*(10+noOfDivisions), angle);
            var (dX, dY) = View.TransformD(1, 1);
            var (doX, doY) = View.TransformD(0, 0);
            Width = (int)Math.Sqrt((dX - doX) * (dX - doX) + (dY - doY) * (dY - doY));
            var (_dX, _dY) = View.TransformD(1, 0);
            Delta = (_dX - doX, _dY - doY);
        }

        public int GetNext(AxisLayerState current) {
            var value = Value(current.Location);
            current.Location.X += Delta.X;
            current.Location.Y += Delta.Y;
            return value;
        }

    public void ResetState(AxisLayerState current, int x, int y) =>
            current.Location = View.TransformI(x, y);

        private int Value((int x, int y) pos) =>
             pos.x % Scaler < Width ? 0xFF : pos.y % Scaler < Width ? 0xFF : 0;
    }
}
