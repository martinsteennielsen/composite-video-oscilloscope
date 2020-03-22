
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

        public int GetNext(PlotState current) {
            var currentValue = Get(current);
            LayerSignal.Next(current.SignalState);
            LayerAxis.Next(current.AxisState);
            return currentValue;
        }
        int Blend(int intensityA, int intensityB, int alpha) =>
            intensityA + ((intensityB - intensityA) * alpha) / 0xFF;

        int Get(PlotState current) {
            int intensityAxis = Blend(Controls.IntensityBackground, Controls.IntensityAxis, alpha: LayerAxis.Get(current.AxisState));
            return Blend(intensityAxis, Controls.IntensitySignal, alpha: LayerSignal.Get(current.SignalState));
        }

        public bool Visible(int x, int y) =>
            Viewport.Visible(x, y);

        public void ResetState(PlotState current, int x, int y) {
            LayerAxis.ResetState(current.AxisState, x, y);
            LayerSignal.ResetState(current.SignalState, x, y);
        }
    }
}
