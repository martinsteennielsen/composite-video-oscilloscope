using System;

namespace CompositeVideoOscilloscope {


    public class Screen {
        readonly Controls Controls;
        readonly InputSignal Signal;
        readonly Location Location1, Location2;

        public Screen(Controller controller, InputSignal signal) {
            Controls = controller.Controls;
            Signal = signal;
            Location1 = new Location { Left = 0.1, Top = 0.1, Right = 0.5, Bottom = 0.5, Angle=0 };
            Location2 = new Location { Left = 0.6, Top = 0.6, Right = 1.0, Bottom = 1.0, Angle=0 };
            
            controller.Movements.Add(0, -0.04, 0, () => Location2.Left, d => Location2.Left += d);
            controller.Movements.Add(0, -0.04, 0, () => Location2.Top, d => Location2.Top += d);
            controller.Movements.Add(Math.PI / 2, 0.4, 0.01, () => Location2.Angle, d => Location2.Angle += d);
            controller.Movements.Add(-Math.PI / 2, -0.2, -0.01, () => Location1.Angle, d => Location1.Angle += d);
        }

        public IContent Content =>
            new FrameContent( 
                new SignalPlot(Location1, Controls.PlotControls, Controls.VideoStandard, Signal.GetSample(0, Controls.CurrentTime, Controls.PlotControls.TriggerVoltage, Controls.PlotControls.TriggerEdge), Controls.CurrentTime),
                new SignalPlot(Location2, Controls.PlotControls, Controls.VideoStandard, Signal.GetSample(1, Controls.CurrentTime, Controls.PlotControls.TriggerVoltage, Controls.PlotControls.TriggerEdge), Controls.CurrentTime) 
            );


        class FrameContent : IContent {
            readonly SignalPlot Plot1, Plot2;

            public FrameContent(SignalPlot plot1, SignalPlot plot2) {
                Plot1 = plot1;
                Plot2 = plot2;
            }

            public int Intensity(int x, int y) {
                if (!Plot1.Visible(x,y) && !Plot2.Visible(x,y)) {
                    return 0x00;
                } 
                if (Plot1.Visible(x,y) && Plot2.Visible(x,y)) {
                    return Blend(Plot1.Intensity(x,y), Plot2.Intensity(x,y), 50);                    
                } 
                if (Plot2.Visible(x,y)) {
                    return Plot2.Intensity(x,y);
                } 
                return Plot1.Intensity(x,y);
            }

            private int Blend(int intensityA, int intensityB, int alpha) => 
                intensityA + ((intensityB - intensityA)*alpha)/0xFF;
        }
    }
}
