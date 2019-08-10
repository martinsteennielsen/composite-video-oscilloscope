﻿using System;

namespace CompositeVideoOscilloscope {

    public interface IScreenContent {
        int PixelValue(int x, int y);
    }

    public class ScreenContent : IScreenContent {
        readonly Timing Timing;
        readonly IScreenContent[] Layers;
        readonly Viewport Screen;
        public ScreenContent(Controls controls, InputSignal signal) {
            Timing = controls.VideoStandard.Timing;

            double fullWidth = Timing.BandwidthFreq / Timing.HFreq;
            double fullHeight =  2 * Timing.HFreq / Timing.VFreq;
            var verticalBorderSize = (int)(Timing.SyncTimes.LineBlankingTime / Timing.DotTime);
            var horizontalBorderSize = 25;

            Screen = new Viewport(0, 0, fullWidth - verticalBorderSize, fullHeight- horizontalBorderSize);

            Layers = new IScreenContent[] { 
                new LayerBackground(), 
                new LayerAxis(Screen, controls.NumberOfDivisions),
                new LayerSignal(Screen, signal, controls)
            };
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
    }
}
