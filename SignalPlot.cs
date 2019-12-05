using System;

namespace CompositeVideoOscilloscope {

    public class SignalPlot {
        readonly Viewport Viewport;
        readonly LayerSignal LayerSignal;
        readonly LayerAxis LayerAxis;
        readonly PlotControls Controls;

        public SignalPlot(LocationControls pos, PlotControls controls, VideoStandard standard, Sampling sampling) {
            Viewport = new Viewport(standard.VisibleWidth * pos.Left, standard.VisibleHeight * pos.Top, standard.VisibleWidth * pos.Right, standard.VisibleHeight * pos.Bottom);
            LayerSignal =  new LayerSignal(Viewport, sampling, controls, pos.Angle, standard);
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
