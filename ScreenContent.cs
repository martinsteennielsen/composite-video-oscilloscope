using System;

namespace CompositeVideoOscilloscope {

    public interface IScreenContent {
        int PixelValue(int x, int y);
    }

    public class ScreenContent : IScreenContent {
        readonly TimingConstants Timing;
        readonly IScreenContent[] Layers;

        public ScreenContent(TimingConstants timing, InputSignal signal) {
            Timing = timing;
            var resolution = new ScreenResolution(timing);
            Layers = new IScreenContent[] { new LayerBackground(), new LayerAxis(resolution, gridPercentage: 7), new LayerSignal(resolution, signal) };
        }

        public int PixelValue(int x, int y) {
            int currentValue = 255;
            foreach (var layer in Layers) {
                currentValue *= layer.PixelValue(x, y);
                currentValue /= 255;
            }
            currentValue *= (255 - Timing.SyncTimes.BlackLevel);
            currentValue /= 255;
            currentValue += Timing.SyncTimes.BlackLevel;
            return currentValue > 255 ? 255 : Math.Max(Timing.SyncTimes.BlackLevel, currentValue);
        }
    }
}
