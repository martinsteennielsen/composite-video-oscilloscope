using System;

namespace CompositeVideoOscilloscope {

    public interface IScreenContent {
        int PixelValue(int x, int y);
        void VSync();
    }

    public class ScreenContent : IScreenContent {
        readonly TimingConstants Timing;
        readonly IScreenContent[] Layers;
        readonly ViewPort Screen;

        public ScreenContent(TimingConstants timing, InputSignal signal) {
            Timing = timing;
            Screen = new ViewPort(0,0, timing.BandwidthFreq/timing.HFreq, 2*timing.HFreq/timing.VFreq );
            Layers = new IScreenContent[] { new LayerBackground(), new LayerAxis(Screen, gridPercentage: 7), new LayerSignal(Screen, signal) };
        }

        public int PixelValue(int x, int y) {
            if (!Screen.Visible(x,y)) { return 0; }

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

        public void VSync() {
            foreach (var layer in Layers) {
                layer.VSync();
            }
        }
    }
}
