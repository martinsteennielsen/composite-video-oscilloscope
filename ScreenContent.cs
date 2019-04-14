using System;

namespace CompositeVideoOscilloscope {

    public interface IScreenContent {
        double PixelValue(int x, int y);
    }

    public class ScreenContent : IScreenContent {
        readonly TimingConstants Timing;
        readonly IScreenContent[] Layers;

        public ScreenContent(TimingConstants timing) {
            Timing = timing;
            var resolution = new ScreenResolution(timing);
            Layers = new IScreenContent[] { new LayerBackground(), new LayerAxis(resolution, gridPercentage: 7), new LayerSignal(resolution) };
        }

        public double PixelValue(int x, int y) {
            double currentValue = 1;
            foreach (var layer in Layers) {
                currentValue *= layer.PixelValue(x, y);
            }
            currentValue *= (1 - Timing.SyncTimes.BlackLevel);
            currentValue += Timing.SyncTimes.BlackLevel;
            return currentValue > 1 ? 1 : Math.Max(Timing.SyncTimes.BlackLevel, currentValue);
        }
    }

}
