using System;

namespace CompositeVideoOscilloscope {


    public class Screen {
        readonly Controls Controls;
        readonly InputSignal Signal;

        readonly Location LocationPlot1, LocationPlot2;

        public Screen(Controller controller, InputSignal signal) {
            Controls = controller.Controls;
            Signal = signal;
            LocationPlot1 = new Location { Left = 0.1, Top = 0.1, Right = 0.5, Bottom = 0.5, Angle=0 };
            LocationPlot2 = new Location { Left = 0.6, Top = 0.6, Right = 1.0, Bottom = 1.0, Angle=0 };
            
            controller.Movements.Add(0, -0.04, 0, () => LocationPlot2.Left, d => LocationPlot2.Left += d);
            controller.Movements.Add(0, -0.04, 0, () => LocationPlot2.Top, d => LocationPlot2.Top += d);
            controller.Movements.Add(Math.PI / 2, 0.4, 0.01, () => LocationPlot2.Angle, d => LocationPlot2.Angle += d);
            controller.Movements.Add(-Math.PI / 2, -0.2, -0.01, () => LocationPlot1.Angle, d => LocationPlot1.Angle += d);
        }

        public IContent Content =>
            new FrameContent( new SignalPlot(LocationPlot1, Controls, Signal), new SignalPlot(LocationPlot2, Controls, Signal));


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
