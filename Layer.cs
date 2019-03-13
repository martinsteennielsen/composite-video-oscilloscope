
namespace CompositeVideoOscilloscope {
    public interface ILayer {
        double PixelValue(int x, int y, double currentValue);
    }

    public abstract class Layer: ILayer {
        protected readonly TimingConstants Timing;
        public Layer(TimingConstants timing) {
            Timing = timing;
        }
        public double PixelValue(int x, int y, double currentValue) {
            return currentValue * Value(x,y);
        }
        protected abstract double Value(double x, double y);
    }

    public class AxisLayer : Layer {
        public AxisLayer(TimingConstants timing) : base(timing) { }

        protected override double Value(double x, double y) {
            if (x % 20 < 0.01) { return .8; }
            if (y % 40 < 0.01) { return .8; }
            return 1;
        }
    }

    public class BackgroundLayer : Layer {
        public BackgroundLayer(TimingConstants timing) : base(timing) { }
        protected override double Value(double x, double y) => .2;
    }

    public class SignalLayer : Layer {
        public SignalLayer(TimingConstants timing) : base(timing) { }
        protected override double Value(double x, double y) => (y/288) * (x/320);
    }

}
