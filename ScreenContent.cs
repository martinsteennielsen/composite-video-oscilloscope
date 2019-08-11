using System;

namespace CompositeVideoOscilloscope {

    public interface IScreenContent {
        int PixelValue(int x, int y);
    }

    public class ScreenContent : IScreenContent {
        readonly int BlackLevel;
        readonly IScreenContent[] Layers;
        readonly Viewport Screen;
        
        public ScreenContent(Controls controls, InputSignal signal) {
            var standard = controls.VideoStandard; 
            BlackLevel = standard.BlackLevel;

            Screen = new Viewport(0, 0, standard.VisibleWidth, standard.VisibleHeight);

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
            currentValue *= (255 - BlackLevel);
            currentValue /= 255;
            currentValue += BlackLevel;
            return currentValue > 255 ? 255 : Math.Max(BlackLevel, currentValue);
        }
    }
}
