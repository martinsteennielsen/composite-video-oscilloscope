using System;

namespace CompositeVideoOscilloscope {
    public class Location {
        public double Left;
        public double Top;
        public double Right;
        public double Bottom;
        public double Angle;
    }

    public class SignalPlot {
        readonly Viewport Viewport;
        readonly LayerSignal LayerSignal;
        readonly LayerAxis LayerAxis;
        readonly PlotControls Controls;

        public SignalPlot(Location pos, PlotControls controls, VideoStandard standard, SignalSample signal, double currentTime) {
            var size = standard.VisibleWidth;
            Viewport = new Viewport(standard.VisibleWidth * pos.Left, standard.VisibleHeight * pos.Top, standard.VisibleWidth * pos.Right, standard.VisibleHeight * pos.Bottom);
            LayerSignal =  new LayerSignal(Viewport, signal, controls, pos.Angle, currentTime);
            LayerAxis =  new LayerAxis(Viewport, controls.NumberOfDivisions, pos.Angle);
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
