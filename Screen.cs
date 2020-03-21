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
                new SignalPlot(Location1, Controls.PlotControls, Controls.VideoStandard, sampling: Aquisition.GetSampling(0, Controls.CurrentTime, Controls.PlotControls)),
                new SignalPlot(Location2, Controls.PlotControls, Controls.VideoStandard, sampling: Aquisition.GetSampling(1, Controls.CurrentTime, Controls.PlotControls))
            );

    }

    public class FrameContent {
        readonly SignalPlot Plot1, Plot2;
        readonly PlotIterator ZeroPlot = new PlotIterator();

        public FrameContent(SignalPlot plot1, SignalPlot plot2) {
            Plot1 = plot1;
            Plot2 = plot2;
        }

        public void Next(ContentIterator iter) {
            iter.CurrentX++;
            var visible1 = Plot1.Visible(iter.CurrentX, iter.CurrentY);
            var visible2 = Plot2.Visible(iter.CurrentX, iter.CurrentY);

            if (visible1 && iter.Plot1 == ZeroPlot) {
                iter.Plot1 = Plot1.Start(iter.CurrentX, iter.CurrentY);
            } else if (!visible1 && iter.Plot1 != ZeroPlot) {
                iter.Plot1 = ZeroPlot;
            }
            if (visible2 && iter.Plot2 == ZeroPlot) {
                iter.Plot2 = Plot2.Start(iter.CurrentX, iter.CurrentY);
            } else if (!visible2 && iter.Plot2 != ZeroPlot) {
                iter.Plot2 = ZeroPlot;
            }
        }

        public ContentIterator Start(int lineNo) =>
            new ContentIterator { CurrentY = lineNo, CurrentX = 0, Plot1 = ZeroPlot, Plot2 = ZeroPlot };

        public int Get(ContentIterator iter) =>
            iter.Plot1 == ZeroPlot && iter.Plot2 == ZeroPlot ? 0
            : iter.Plot1 == ZeroPlot ? Plot2.GetNext(iter.Plot2)
            : iter.Plot2 == ZeroPlot ? Plot1.GetNext(iter.Plot1)
            : Blend(Plot1.GetNext(iter.Plot1), Plot2.GetNext(iter.Plot2), 50);

        private int Blend(int intensityA, int intensityB, int alpha) =>
            intensityA + ((intensityB - intensityA) * alpha) / 0xFF;
    }
}

