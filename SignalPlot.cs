using System;

namespace CompositeVideoOscilloscope {
    public class Location {
        public double Left;
        public double Top;
        public double Right;
        public double Bottom;
        public double Angle=0;
    }

    public class SignalPlot {
        readonly Viewport Viewport;
        readonly LayerSignal LayerSignal;
        readonly LayerAxis LayerAxis;

        readonly Controls Controls;

        public SignalPlot(Location pos, Controls controls, InputSignal signal) {
            var standard = controls.VideoStandard; 
            var size = standard.VisibleWidth;
            Viewport = new Viewport(standard.VisibleWidth * pos.Left, standard.VisibleHeight * pos.Top, standard.VisibleWidth * pos.Right, standard.VisibleHeight * pos.Bottom);
            LayerSignal =  new LayerSignal(Viewport, signal, controls.PlotControls, pos.Angle, controls.CurrentTime);
            LayerAxis =  new LayerAxis(Viewport, controls.PlotControls.NumberOfDivisions, pos.Angle);
            Controls = controls;
        }

        public bool Visible(int x, int y) =>
            Viewport.Visible(x,y);

        public int Intensity(int x, int y) {
            int intensityAxis = Blend(Controls.IntensityBackground, Controls.IntensityAxis, alpha: LayerAxis.Intensity(x,y));
            return Blend(intensityAxis, Controls.IntensitySignal, alpha: LayerSignal.Intensity(x,y));
        }

        int Blend(int intensityA, int intensityB, int alpha) => 
            intensityA + ((intensityB - intensityA)*alpha)/0xFF;

    }
}
