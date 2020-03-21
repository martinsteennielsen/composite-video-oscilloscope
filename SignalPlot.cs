
namespace CompositeVideoOscilloscope {

    using static LayerAxisI;
    using static LayerSignalI;
    using static SamplingI;

    public class SignalPlotI {
        LayerAxisI Axis;
        LayerSignalI Signal;

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

            public int GetNext(SignalPlotI iter) {
                var current = Get(iter);
                LayerSignal.Next(iter.Signal);
                LayerAxis.Next(iter.Axis);
                return current;
            }
            int Blend(int intensityA, int intensityB, int alpha) =>
                intensityA + ((intensityB - intensityA) * alpha) / 0xFF;

            int Get(SignalPlotI iter) {
                int intensityAxis = Blend(Controls.IntensityBackground, Controls.IntensityAxis, alpha: LayerAxis.Get(iter.Axis));
                return Blend(intensityAxis, Controls.IntensitySignal, alpha: LayerSignal.Get(iter.Signal));
            }

            public bool Visible(int x, int y) =>
                Viewport.Visible(x, y);

            public SignalPlotI CreateIterator(int x, int y) =>
                new SignalPlotI { Axis = LayerAxis.Start(x, y), Signal = LayerSignal.Start(x, y) };

        }
    }
}