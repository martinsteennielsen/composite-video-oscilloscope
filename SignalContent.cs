using System;

namespace CompositeVideoOscilloscope {
    public class SignalContent {
        readonly Viewport Viewport;
        readonly LayerSignal LayerSignal;
        readonly LayerAxis LayerAxis;

        readonly Controls Controls;

        public SignalContent(Controls controls, InputSignal signal) {
            var standard = controls.VideoStandard; 
            var size = standard.VisibleWidth;
            var (l,t,r,b) = controls.ScreenPosition;
            Viewport = new Viewport(standard.VisibleWidth * l, standard.VisibleHeight * t, standard.VisibleWidth * r, standard.VisibleHeight * b);

            LayerSignal =  new LayerSignal(Viewport, signal, controls);
            LayerAxis =  new LayerAxis(Viewport, controls.NumberOfDivisions, controls.Angle);
            Controls = controls;
        }

        public bool Visible(int x, int y) =>
            Viewport.Visible(x,y);

        public int Intensity(int x, int y) {
            int intensityAxis = Blend(Controls.IntensityBackground, Controls.IntensityAxis, alpha: LayerAxis.Intensity(x,y));
            return Blend(intensityAxis, Controls.IntensitySignal, alpha: LayerSignal.Intensity(x,y));
        }

        private int Blend(int intensityA, int intensityB, int alpha) => 
            intensityA + ((intensityB - intensityA)*alpha)/0xFF;

    }
}
