
namespace CompositeVideoOscilloscope {
    public class LayerSignal : IScreenContent {
        private readonly View View;
        private readonly InputSignal Signal;
        private readonly double dT, dV, d2T, d2v;

        public LayerSignal(ScreenResolution resolution, InputSignal signal) {
            Signal = signal;
            View = new View(20, 30, -2, 2, resolution: new ScreenResolution(2 * resolution.Width, 2 * resolution.Height));
            dT=View.Scaler.dX;
            d2T=2*dT;
            dV = View.Scaler.dY;
            d2v=2*dV;
        }

        public int PixelValue(int x, int y) => Value(x << 1, y << 1);

        private int Value(int x, int y) {
            (double t, double v) = View.Transform(x, y);
            double[] ss;
            if (!Signal.TryGet(t,dT,out ss)) { return 255; }
            double vu = v-dV, vuu = v-d2v, vd = v+dV, vdd = v+d2v;
            bool dr=ss[3]-v>0, dd=ss[2]-vd>0, du=ss[2]-vu>0, dl=ss[1]-v>0;

            int r=0;
            r += ( IsOff(dd,       du,       dl,       dr)            ? 1:4 ) << 2;
            r +=   IsOff(dl,       ss[1]-vuu>0, ss[0]-vu>0, du)       ? 1:4;
            r +=   IsOff(dr,       ss[3]-vuu>0, du,       ss[4]-vu>0) ? 1:4;
            r +=   IsOff(ss[1]-vdd>0, dl,       ss[0]-vd>0, dd)       ? 1:4;
            r +=   IsOff(ss[3]-vdd>0, dr,       dd,       ss[4]-vd>0) ? 1:4;
            return r << 5;
        }

        private bool IsOff(bool dd, bool du, bool dl, bool dr) => 
            (dd && du && dl && dr) || !(dd || du || dl || dr);
    }
}
