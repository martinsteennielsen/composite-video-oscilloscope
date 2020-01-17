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

        public IContent Content =>
            new FrameContent(
                new SignalPlot(Location1, Controls.PlotControls, Controls.VideoStandard, sampling: Aquisition.GetSampling(0, Controls.CurrentTime, Controls.PlotControls)),
                new SignalPlot(Location2, Controls.PlotControls, Controls.VideoStandard, sampling: Aquisition.GetSampling(1, Controls.CurrentTime, Controls.PlotControls))
            );

    }
    public interface IIterator {
        int GetNext();
    }

    class ZeroIterator : IIterator {
        public int GetNext() => 0;
    }

    class FrameContent : IContent {
        readonly SignalPlot Plot1, Plot2;
        readonly IIterator ZeroIterator = new ZeroIterator();

        int CurrentX, CurrentY;
        IIterator Plot1Iterator, Plot2Iterator; 

        public FrameContent(SignalPlot plot1, SignalPlot plot2) {
            Plot1 = plot1;
            Plot2 = plot2;
            Plot1Iterator = Plot2Iterator = ZeroIterator;
        }

        public void Next() {
            CurrentX++;
            var visible1 = Plot1.Visible(CurrentX, CurrentY);
            var visible2 = Plot2.Visible(CurrentX, CurrentY);

            if (visible1 && Plot1Iterator == ZeroIterator) {
                Plot1Iterator = Plot1.Start(CurrentX, CurrentY);
            } else if (!visible1 && Plot1Iterator != ZeroIterator) {
                Plot1Iterator = ZeroIterator;
            }
            if (visible2 && Plot2Iterator == ZeroIterator) {
                Plot2Iterator = Plot2.Start(CurrentX, CurrentY);
            } else if (!visible2 && Plot2Iterator != ZeroIterator) {
                Plot2Iterator = ZeroIterator;
            }
        }

        public void Start(int lineNo) {
            CurrentY=lineNo;
            CurrentX=0;
        }

        public int Get() =>
            Plot1Iterator == ZeroIterator 
            ? Plot2Iterator.GetNext()
            : Blend(Plot1Iterator.GetNext(), Plot2Iterator.GetNext(), 50);

        private int Blend(int intensityA, int intensityB, int alpha) =>
            intensityA + ((intensityB - intensityA) * alpha) / 0xFF;
    }
}
