namespace CompositeVideoOscilloscope {

    public class SignalPlot {
        readonly Viewport Viewport;
        readonly LayerSignal LayerSignal;
        readonly LayerAxis LayerAxis;
        readonly PlotControls Controls;

        private int CurrentX, CurrentY;

        public SignalPlot(LocationControls pos, PlotControls controls, VideoStandard standard, Sampling sampling) {
            Viewport = new Viewport(standard.VisibleWidth * pos.Left, standard.VisibleHeight * pos.Top, standard.VisibleWidth * pos.Right, standard.VisibleHeight * pos.Bottom);
            LayerSignal =  new LayerSignal(Viewport, sampling, controls, pos.Angle, standard);
            LayerAxis =  new LayerAxis(Viewport, controls.NumberOfDivisions, pos.Angle);
            Controls = controls;
            CurrentX = CurrentY = 0;
        }


        public void Next() {
            CurrentX++;
            LayerAxis.Next();
            LayerSignal.Next();
        }

        public void NewLine(int lineNo) {
            CurrentX = 0;
            CurrentY = lineNo;
            LayerAxis.NewLine(lineNo);
            LayerSignal.NewLine(lineNo);
        }

        public bool Visible() =>
            Viewport.Visible(CurrentX,CurrentY);

        public int Intensity() {
            int intensityAxis = Blend(Controls.IntensityBackground, Controls.IntensityAxis, alpha: LayerAxis.Intensity());
            return Blend(intensityAxis, Controls.IntensitySignal, alpha: LayerSignal.Get());
        }

        int Blend(int intensityA, int intensityB, int alpha) => 
            intensityA + ((intensityB - intensityA)*alpha)/0xFF;

    }
}
