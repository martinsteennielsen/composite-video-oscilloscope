
using System;

namespace CompositeVideoOscilloscope {
    public class LayerSignal : IScreenContent {

        private readonly Viewport View, Screen;
        private readonly InputSignal Signal;
        private readonly double dT, dV, d2T, d2V;
        
        public LayerSignal(Viewport screen, InputSignal signal, Controls controls) {
            Screen = screen.SetView(0,0,2*screen.Width, 2*screen.Height);
            Signal = signal;

            var divisionsPrQuadrant = controls.NumberOfDivisions/2;
            
            View = new Viewport(0,0, Screen.Right, Screen.Bottom).SetView(
                controls.CurrentTime + controls.Position.Time + signal.TriggerOffsetTime,
                controls.Position.Voltage + controls.Units.Voltage*divisionsPrQuadrant, 
                controls.CurrentTime + signal.TriggerOffsetTime + controls.Position.Time + controls.Units.Time*controls.NumberOfDivisions,
                controls.Position.Voltage - controls.Units.Voltage*divisionsPrQuadrant);

            var (half, origo) = (View.Transform(1, 1),  View.Transform(0,0));
            (dT,dV) = (half.x-origo.x, half.y-origo.y);
            (d2T, d2V) = (2*dT, 2*dV);
        }

        public int PixelValue(int x, int y) =>
            PixelValue(View.Transform(Screen.Transform(x,y)));

        private int PixelValue((double t, double v) position) {
            if (!Signal.TryGet(position.t, dT, out var sigbuf)) { return 0xff; }

            double v = position.v;
            double vu = v - dV, vuu = v - d2V, vd = v + dV, vdd = v + d2V;
            bool dr = sigbuf[3] - v > 0, dd = sigbuf[2] - vd > 0, du = sigbuf[2] - vu > 0, dl = sigbuf[1] - v > 0;

            int r = Pixel(dl, sigbuf[1] - vuu > 0, sigbuf[0] - vu > 0, du);
            r += Pixel(dr, sigbuf[3] - vuu > 0, du, sigbuf[4] - vu > 0);
            r += Pixel(sigbuf[1] - vdd > 0, dl, sigbuf[0] - vd > 0, dd);
            r += Pixel(sigbuf[3] - vdd > 0, dr, dd, sigbuf[4] - vd > 0);
            r += Pixel(dd, du, dl, dr)  << 2;
            return r << 5;
        }

        private int Pixel(bool dd, bool du, bool dl, bool dr) =>
            (dd && du && dl && dr) || !(dd || du || dl || dr) ? 1 : 4;
    }
}
