using System.Collections.Generic;

namespace CompositeVideoOscilloscope {

    public class LayerSignal {
        private readonly Viewport View;
        private readonly Sampling Sample;
        private readonly double dV, d2V;
        private readonly int dTns, d2Tns;
        private readonly (int X, double Y) Delta;
        
        private (int X, double Y) Current = (0, 0);

        public LayerSignal(Viewport screen, Sampling sample, PlotControls controls, double angle, VideoStandard standard) {
            Sample = sample;

            var divisionsPrQuadrant = controls.NumberOfDivisions/2;

            View = screen.SetView(
                controls.Position.TimeNs + sample.TriggerTimeNs,
                controls.Position.Voltage + controls.Units.Voltage * divisionsPrQuadrant,
                sample.TriggerTimeNs + controls.Position.TimeNs + controls.Units.TimeNs * controls.NumberOfDivisions,
                controls.Position.Voltage - controls.Units.Voltage * divisionsPrQuadrant, angle);

            (dTns,dV) = ((int)(View.Width / (screen.Width*2)), View.Height / (screen.Height*2));
            (d2Tns, d2V) = (2*dTns, 2*dV);

            var (doX, doY) = View.Transform(0, 0);
            var (dX, dY) = View.Transform(1, 0);
            Delta = ((int)(dX - doX), dY - doY);
        }

        public void Next() {
            Current.X += Delta.X;
            Current.Y += Delta.Y;
        }

        public void NewLine(int lineNo) {
            var _c = View.Transform(0, lineNo);
            Current = ((int)_c.x, _c.y);
        }
        public int Intensity() =>
            PixelValue( Current );

        private int PixelValue((int tns, double v) position) {
            double[] sigbuf = new double[5];
            if (!TryGet(position.tns, ref sigbuf)) { return 0x00; }

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

        private bool TryGet(int tns, ref double[] value) =>
            Sample.TryGet(tns - d2Tns, out value[0]) &&
            Sample.TryGet(tns - dTns, out value[1]) &&
            Sample.TryGet(tns, out value[2]) &&
            Sample.TryGet(tns + dTns, out value[3]) &&
            Sample.TryGet(tns + d2Tns, out value[4]);
    }
}
