
namespace CompositeVideoOscilloscope {
    public class LayerSignal : IScreenContent {
        private readonly View View;
        private readonly InputSignal Signal;

        public LayerSignal(ScreenResolution resolution, InputSignal signal) {
            Signal = signal;
            View = new View(-20, 40, -2, 2, resolution: new ScreenResolution(2 * resolution.Width, 2 * resolution.Height));
        }

        public int PixelValue(int x, int y) => Value(x << 1, y << 1);

        private int Value(int x, int y) {
            (double time, double voltage) = View.Transform(x, y);

            int s = IsPixelOn(time - View.Scaler.dX, voltage - View.Scaler.dY) ? 4: 1;
            s += IsPixelOn(time + View.Scaler.dX, voltage - View.Scaler.dY) ? 4: 1;
            s += IsPixelOn(time - View.Scaler.dX, voltage + View.Scaler.dY) ? 4: 1;
            s += IsPixelOn(time + View.Scaler.dX, voltage + View.Scaler.dY) ? 4: 1;
            s += (IsPixelOn(time, voltage) ? 4:1) << 2;
            return s << 5;
        }

        private bool IsPixelOn(double time, double voltage) {
            if (!Signal.TryGet(time, out double value)) { return false; }
            sbyte s = value - voltage - View.Scaler.dY > 0 ? (sbyte)1 : (sbyte)-1;
            s += value - voltage + View.Scaler.dY > 0 ? (sbyte)1 : (sbyte)-1;
            s += (Signal.TryGet(time - View.Scaler.dX, out double v1) ? v1 : value) - voltage > 0 ? (sbyte)1 : (sbyte)-1;
            s += (Signal.TryGet(time + View.Scaler.dX, out double v2) ? v2 : value) - voltage > 0 ? (sbyte)1 : (sbyte)-1;
            return s != 4 && s != -4;
        }
    }
}
