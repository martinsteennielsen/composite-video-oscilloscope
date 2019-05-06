
using System;

namespace CompositeVideoOscilloscope {
    public class LayerSignal : IScreenContent {
        private readonly View View;
        private readonly InputSignal Signal;
        private readonly double dT, dV, d2T, d2V;
        private readonly double[] SigBuf;
        private int MinX =2, MaxX;

        public LayerSignal(ScreenResolution resolution, InputSignal signal) {
            Signal = signal;
            View = new View(20, 40, -2, 2, resolution: new ScreenResolution(2 * resolution.Width, 2 * resolution.Height));
            SigBuf = new double[2 * resolution.Width];
            dT = View.Scaler.dX;
            dV = View.Scaler.dY;
            d2T = 2 * dT;
            d2V = 2 * dV;
        }

        public void VSync() {
            MinX=int.MaxValue; MaxX=int.MinValue;
            for (int x=0; x<View.Resolution.Width; x++) {
                if (Signal.TryGet(time: View.Transform(x, 0).outX, value: out SigBuf[x])) {
                    if (x>MaxX) { MaxX =x;}
                    if (x<MinX) { MinX =x;}
                }
            }
            MinX = Math.Max(2, MinX);
            MaxX = Math.Min(SigBuf.Length-2, MaxX);
        }

        public int PixelValue(int x, int y) => Value(x << 1, y << 1);

        private int Value(int x, int y) {
            if (x < MinX  || x > MaxX ) { return 255; }

            double v = View.Transform(x, y).outY;
            double vu = v - dV, vuu = v - d2V, vd = v + dV, vdd = v + d2V;
            bool dr = SigBuf[x+1] - v > 0, dd = SigBuf[x] - vd > 0, du = SigBuf[x] - vu > 0, dl = SigBuf[x-1] - v > 0;
            int r = 0;
            r += IsOff(dl, SigBuf[x-1] - vuu > 0, SigBuf[x-2] - vu > 0, du);
            r += IsOff(dr, SigBuf[x+1] - vuu > 0, du, SigBuf[x+2] - vu > 0);
            r += IsOff(SigBuf[x-1] - vdd > 0, dl, SigBuf[x-2] - vd > 0, dd);
            r += IsOff(SigBuf[x+1] - vdd > 0, dr, dd, SigBuf[x+2] - vd > 0);
            r += IsOff(dd, du, dl, dr)  << 2;
            return r << 5;
        }

        private int IsOff(bool dd, bool du, bool dl, bool dr) =>
            (dd && du && dl && dr) || !(dd || du || dl || dr) ? 1 : 4;
    }
}
