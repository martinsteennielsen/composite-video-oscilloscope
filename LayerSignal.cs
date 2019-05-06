
namespace CompositeVideoOscilloscope {
    public class LayerSignal : IScreenContent {
        private readonly View View;
        private readonly InputSignal Signal;
        private readonly double dT, dV, d2T, d2V;
        private readonly double[] Values;
        private readonly bool[] HasValue;

        public LayerSignal(ScreenResolution resolution, InputSignal signal) {
            Signal = signal;
            View = new View(20, 80, -2, 2, resolution: new ScreenResolution(2 * resolution.Width, 2 * resolution.Height));
            Values = new double[2 * resolution.Width];
            HasValue = new bool[2 * resolution.Width];
            dT = View.Scaler.dX;
            d2T = 2 * dT;
            dV = View.Scaler.dY;
            d2V = 2 * dV;
        }

        public void VSync() {
            for (int x=0; x<View.Resolution.Width; x++) {
                HasValue[x] = Signal.TryGet(View.Transform(x, 0).outX, out Values[x]);
            }
        }

        public int PixelValue(int x, int y) => Value(x << 1, y << 1);

        private int Value(int x, int y) {
            if (x<2 || !HasValue[x - 2] || !HasValue[x + 2]) { return 255; }

            double v = View.Transform(x, y).outY;
            double vu = v - dV, vuu = v - d2V, vd = v + dV, vdd = v + d2V;
            bool dr = Values[x+1] - v > 0, dd = Values[x] - vd > 0, du = Values[x] - vu > 0, dl = Values[x-1] - v > 0;
            int r = 0;
            r += (IsOff(dd, du, dl, dr) ? 1 : 4) << 2;
            r += IsOff(dl, Values[x - 1] - vuu > 0, Values[x - 2] - vu > 0, du) ? 1 : 4;
            r += IsOff(dr, Values[x + 1] - vuu > 0, du, Values[x + 2] - vu > 0) ? 1 : 4;
            r += IsOff(Values[x - 1] - vdd > 0, dl, Values[x - 2] - vd > 0, dd) ? 1 : 4;
            r += IsOff(Values[x + 1] - vdd > 0, dr, dd, Values[x + 2] - vd > 0) ? 1 : 4;
            return r << 5;
        }

        private bool IsOff(bool dd, bool du, bool dl, bool dr) =>
            (dd && du && dl && dr) || !(dd || du || dl || dr);
    }
}
