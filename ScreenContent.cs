using System;

namespace CompositeVideoOscilloscope {

    public interface IScreenContent {
        int PixelValue(int x, int y);
        void VSync();
    }

    public class ScreenContent : IScreenContent {
        readonly TimingConstants Timing;
        readonly IScreenContent[] Layers;
        readonly Viewport Screen;
        public ScreenContent(TimingConstants timing, Controls controls, InputSignal signal) {
            Timing = timing;

            double fullWidth = timing.BandwidthFreq / timing.HFreq;
            double fullHeight =  2 * timing.HFreq / timing.VFreq;
            var verticalBorderSize = (int)(timing.SyncTimes.LineBlankingTime / timing.DotTime);
            var horizontalBorderSize = 25;

            Screen = new Viewport(0, 0, fullWidth - verticalBorderSize, fullHeight- horizontalBorderSize);
            Layers = new IScreenContent[] { new LayerBackground(), new LayerAxis(Screen, noOfDivisions: controls.NoOfDivisions), new LayerSignal(Screen, signal, controls) };
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
