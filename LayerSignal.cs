
using System;

namespace CompositeVideoOscilloscope {
    public class LayerSignal : IScreenContent {
        private readonly Viewport View, Screen;
        private readonly InputSignal Signal;
        private readonly double dT, dV, d2T, d2V;
        private readonly double[] SigBuf;
        private readonly int SignalMinX, SignalMaxX;

        public LayerSignal(Viewport screen, InputSignal signal, Controls controls) {
            Signal = signal;
            Screen = screen;
            SigBuf = new double[(int)(2 * screen.Width)];
            View = SetView(screen, controls);
            var (half, origo) = (View.Transform(0.5, 0.5),  View.Transform(0,0));
            (dT,dV) = (half.x-origo.x, half.y-origo.y);
            (d2T, d2V) = (2*dT, 2*dV);
            (SignalMinX, SignalMaxX) = ExamineSignal();
        }

        private Viewport SetView(Viewport screen, Controls controls) {
            var divisionsPrQuadrant = controls.NumberOfDivisions/2;
            return screen.SetView(0, controls.Units.Voltage*divisionsPrQuadrant, controls.Units.Time*controls.NumberOfDivisions, -controls.Units.Voltage*divisionsPrQuadrant);
        }

        private (int,int) ExamineSignal() {
            var min=int.MaxValue;
            var max=int.MinValue;
            double time = View.Left;
            for (int pos=0; pos<SigBuf.Length; pos++) {
                if (Signal.TryGet(time: time, value: out SigBuf[pos])) {
                    if (pos>max) { max =pos;}
                    if (pos<min) { min =pos;}
                }
                time += dT;
            }
            return (Math.Max(2, min), Math.Min(SigBuf.Length-2, max));
        }

        public int PixelValue(int x, int y) => Value((x-(int)Screen.Left)*2, y*2);

        private int Value(int x, int y) {
            if (x <= SignalMinX  || x >= SignalMaxX ) { return 255; }

            double v = View.Transform(0, y/2).y;
            double vu = v - dV, vuu = v - d2V, vd = v + dV, vdd = v + d2V;
            bool dr = SigBuf[x+1] - v > 0, dd = SigBuf[x] - vd > 0, du = SigBuf[x] - vu > 0, dl = SigBuf[x-1] - v > 0;

            int r = Pixel(dl, SigBuf[x-1] - vuu > 0, SigBuf[x-2] - vu > 0, du);
            r += Pixel(dr, SigBuf[x+1] - vuu > 0, du, SigBuf[x+2] - vu > 0);
            r += Pixel(SigBuf[x-1] - vdd > 0, dl, SigBuf[x-2] - vd > 0, dd);
            r += Pixel(SigBuf[x+1] - vdd > 0, dr, dd, SigBuf[x+2] - vd > 0);
            r += Pixel(dd, du, dl, dr)  << 2;
            return r << 5;
        }

        private int Pixel(bool dd, bool du, bool dl, bool dr) =>
            (dd && du && dl && dr) || !(dd || du || dl || dr) ? 1 : 4;
    }
}
