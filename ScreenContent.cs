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
            var size = standard.VisibleWidth;
            var (l,t,r,b) = controls.ScreenPosition;

            Screen = new Viewport(size*l, size*t, standard.VisibleWidth*r, standard.VisibleHeight*b);

            Layers = new IScreenContent[] { 
                new LayerBackground(), 
                new LayerAxis(Screen, controls.NumberOfDivisions),
                new LayerSignal(Screen, signal, controls)
            };
        }

        public int PixelValue(int x, int y) {
            if (!Screen.Visible(x,y)) { return BlackLevel; }

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
