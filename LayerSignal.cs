
using System;

namespace CompositeVideoOscilloscope {

    public class LayerSignal {

        private readonly Viewport View;
        private readonly InputSignal Signal;
        private readonly double dT, dV, d2T, d2V;
        
        public LayerSignal(Viewport screen, InputSignal signal, Controls controls) {
            Signal = signal;

            var divisionsPrQuadrant = controls.NumberOfDivisions/2;

            View = screen.SetView(
                controls.CurrentTime + controls.Position.Time + signal.TriggerOffsetTime,
                controls.Position.Voltage + controls.Units.Voltage * divisionsPrQuadrant,
                controls.CurrentTime + signal.TriggerOffsetTime + controls.Position.Time + controls.Units.Time * controls.NumberOfDivisions,
                controls.Position.Voltage - controls.Units.Voltage * divisionsPrQuadrant, controls.Angle);

            (dT,dV) = (View.Width / (screen.Width*2), View.Height / (screen.Height*2));
            (d2T, d2V) = (2*dT, 2*dV);
        }

        public int Intensity(int x, int y) =>
            PixelValue(View.Transform(x,y));


        static double[] sigbuf = new double[5];

        private int PixelValue((double t, double v) position) {
            if (!TryGet(position.t, dT, ref sigbuf)) { return 0; }

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
            (dd && du && dl && dr) || !(dd || du || dl || dr) ? 0 : 1;

        private bool TryGet(double time, double dt, ref double[] value) =>
            Signal.TryGet(time-d2T, out value[0]) &&
            Signal.TryGet(time-dT, out value[1]) &&
            Signal.TryGet(time, out value[2]) &&
            Signal.TryGet(time+dT, out value[3]) &&
            Signal.TryGet(time+d2T, out value[4]);
    }
}
