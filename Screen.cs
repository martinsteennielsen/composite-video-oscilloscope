using System;
using System.Collections.Generic;

namespace CompositeVideoOscilloscope {

    public class Screen {
        readonly Controls Controls;
        readonly Aquisition Aquisition;
        readonly LocationControls Location1, Location2;

        public Screen(Controller controller, Aquisition aquisition) {
            Controls = controller.Controls;
            Aquisition = aquisition;
            Location1 = new LocationControls { Left = 0.1, Top = 0.1, Right = 0.5, Bottom = 0.5, Angle = 0 };
            Location2 = new LocationControls { Left = 0.6, Top = 0.6, Right = 1.0, Bottom = 1.0, Angle = 0 };

            controller.Movements.Add(0, -0.04, 0, () => Location2.Left, d => Location2.Left += d);
            controller.Movements.Add(0, -0.04, 0, () => Location2.Top, d => Location2.Top += d);
            controller.Movements.Add(Math.PI / 2, 0.4, 0.01, () => Location2.Angle, d => Location2.Angle += d);
            controller.Movements.Add(-Math.PI / 2, -0.2, -0.01, () => Location1.Angle, d => Location1.Angle += d);
        }

        public FrameContent Content =>
            new FrameContent(
                new SignalPlot(Location1, Controls.PlotControls, Controls.VideoStandard, sampling: Aquisition.GetSampling(0, Controls.CurrentTime)),
                new SignalPlot(Location2, Controls.PlotControls, Controls.VideoStandard, sampling: Aquisition.GetSampling(1, Controls.CurrentTime))
            );

    }

    public class FrameContent {
        readonly SignalPlot Plot1, Plot2;

        public FrameContent(SignalPlot plot1, SignalPlot plot2) {
            Plot1 = plot1;
            Plot2 = plot2;
        }

        public void Next(ContentState current) {
            current.LocationX++;
            bool visible1 = Plot1.Visible(current.LocationX, current.LocationY);
            bool visible2 = Plot2.Visible(current.LocationX, current.LocationY);

            if (visible1 && !current.Plot1Visible) {
               Plot1.ResetState(current.Plot1State, current.LocationX, current.LocationY);
                current.Plot1Visible = true;
            } else if (!visible1 && current.Plot1Visible) {
                current.Plot1Visible = false;
            }
            if (visible2 && !current.Plot2Visible) {
                Plot2.ResetState(current.Plot2State, current.LocationX, current.LocationY);
                current.Plot2Visible = true;
            } else if (!visible2 && current.Plot2Visible) {
                current.Plot2Visible = false;
            }
        }

        public void ResetState(ContentState current, int lineNo) {
            current.LocationY = lineNo;
            current.LocationX = 0;
            current.Plot1Visible = false;
            current.Plot2Visible = false;
            Plot1.ResetState(current.Plot1State, 0, lineNo);
            Plot2.ResetState(current.Plot2State, 0, lineNo);
        }

        
        public int Get(ContentState current) =>
            !current.Plot1Visible  && !current.Plot2Visible  ? 0
            : !current.Plot1Visible ? Plot2.GetNext(current.Plot2State)
            : !current.Plot2Visible ? Plot1.GetNext(current.Plot1State)
            : Blend(Plot1.GetNext(current.Plot1State), Plot2.GetNext(current.Plot2State), 50);

        private int Blend(int intensityA, int intensityB, int alpha) =>
            intensityA + ((intensityB - intensityA) * alpha) / 0xFF;
    }
}

