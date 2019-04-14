using System;
using System.Collections.Generic;
using System.Text;

namespace CompositeVideoOscilloscope {

    public interface IScreenContent {
        double PixelValue(int x, int y);
    }

    public class ScreenContent : IScreenContent {
        readonly TimingConstants Timing;
        readonly IScreenContent[] Layers;

        public ScreenContent(TimingConstants timing) {
            Timing = timing;
            Layers = new IScreenContent[] { new LayerBackground(), new LayerAxis(timing, gridPercentage: 7), new LayerSignal(timing) };

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
