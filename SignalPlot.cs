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

        public IIterator CreateIterator(int x, int y) =>
            new PlotIterator(Controls, LayerAxis, LayerSignal, x, y);


        class PlotIterator : IIterator {
            readonly LayerAxis Axis;
            readonly LayerSignal Signal;
            readonly PlotControls Controls;

            public PlotIterator(PlotControls controls, LayerAxis axis, LayerSignal signal, int x, int y) {
                Axis = axis;
                Signal = signal;
                Controls = controls; 
                Axis.Start(x,y);
                Signal.Start(x,y);
            }

            int Blend(int intensityA, int intensityB, int alpha) => 
                intensityA + ((intensityB - intensityA)*alpha)/0xFF;

            int Get() {
                int intensityAxis = Blend(Controls.IntensityBackground, Controls.IntensityAxis, alpha: Axis.Get());
                return Blend(intensityAxis, Controls.IntensitySignal, alpha: Signal.Get());
            }

            public int GetNext() {
                var current = Get();
                Signal.Next();
                Axis.Next();
                return current;
            }

        }

    }
}
