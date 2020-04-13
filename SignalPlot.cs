
namespace CompositeVideoOscilloscope {

    public class SignalPlot {
        readonly Viewport Viewport;
        readonly LayerSignal LayerSignal;
        readonly LayerAxis LayerAxis;
        readonly PlotControls Controls;

        public SignalPlot(PlotControls controls, VideoConstants constants, Sampling sampling) {
            Viewport = new Viewport(constants.VisibleWidth * controls.Location.Left, constants.VisibleHeight * controls.Location.Top, constants.VisibleWidth * controls.Location.Right, constants.VisibleHeight * controls.Location.Bottom);
            LayerSignal = new LayerSignal(Viewport, sampling, controls, controls.Location.Angle);
            LayerAxis = new LayerAxis(Viewport, controls.NumberOfDivisions, controls.Location.Angle);
            Controls = controls;
        }

        int Blend(int intensityA, int intensityB, int alpha) =>
            intensityA + ((intensityB - intensityA) * alpha) / 0xFF;

        public int GetNext(PlotState current) {
            int intensityAxis = Blend(Controls.IntensityBackground, Controls.IntensityAxis, alpha: LayerAxis.GetNext(current.AxisState));
            return Blend(intensityAxis, Controls.IntensitySignal, alpha: LayerSignal.GetNext(current.SignalState));
        }

        public bool Visible(int x, int y) =>
            Viewport.Visible(x, y);

        public void ResetState(PlotState current, int x, int y) {
            current.Visible = false;
            LayerAxis.ResetState(current.AxisState, x, y);
            LayerSignal.ResetState(current.SignalState, x, y);
        }
    }
}
