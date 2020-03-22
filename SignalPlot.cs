
namespace CompositeVideoOscilloscope {

    public class SignalPlot {
        readonly Viewport Viewport;
        readonly LayerSignal LayerSignal;
        readonly LayerAxis LayerAxis;
        readonly PlotControls Controls;

        public SignalPlot(LocationControls pos, PlotControls controls, VideoStandard standard, Sampling sampling) {
            Viewport = new Viewport(standard.VisibleWidth * pos.Left, standard.VisibleHeight * pos.Top, standard.VisibleWidth * pos.Right, standard.VisibleHeight * pos.Bottom);
            LayerSignal = new LayerSignal(Viewport, sampling, controls, pos.Angle, standard);
            LayerAxis = new LayerAxis(Viewport, controls.NumberOfDivisions, pos.Angle);
            Controls = controls;
        }

        public int GetNext(PlotIterator iter) {
            var current = Get(iter);
            LayerSignal.Next(iter.Signal);
            LayerAxis.Next(iter.Axis);
            return current;
        }
        int Blend(int intensityA, int intensityB, int alpha) =>
            intensityA + ((intensityB - intensityA) * alpha) / 0xFF;

        int Get(PlotIterator iter) {
            int intensityAxis = Blend(Controls.IntensityBackground, Controls.IntensityAxis, alpha: LayerAxis.Get(iter.Axis));
            return Blend(intensityAxis, Controls.IntensitySignal, alpha: LayerSignal.Get(iter.Signal));
        }

        public bool Visible(int x, int y) =>
            Viewport.Visible(x, y);

        public void Reset(PlotIterator iter, int x, int y) {
            LayerAxis.Reset(iter.Axis, x, y);
            LayerSignal.Reset(iter.Signal, x, y);
        }
    }
}
