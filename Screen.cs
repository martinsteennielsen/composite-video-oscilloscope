using System;
using System.Collections.Generic;
using System.Linq;

namespace CompositeVideoOscilloscope {

    public class Screen {
        readonly Controls Controls;
        readonly Aquisition Aquisition;

        public Screen(Controller controller, Aquisition aquisition) {
            Controls = controller.Controls;
            Aquisition = aquisition;
        }

        public FrameContent Content =>
            new FrameContent(
                new SignalPlot(Controls.Plot1, VideoConstants.Get(Controls.VideoStandard), sampling: Aquisition.GetSampling(Controls.Plot1, 0, Controls.CurrentTime)),
                new SignalPlot(Controls.Plot2, VideoConstants.Get(Controls.VideoStandard), sampling: Aquisition.GetSampling(Controls.Plot2, 1, Controls.CurrentTime))
            );

    }

    public class FrameContent {
        readonly SignalPlot[] Plots;

        public FrameContent(params SignalPlot[] plots) {
            Plots = plots;
        }

        public void Next(ContentState current) {
            current.LocationX++;
            for (int p = 0; p < Plots.Length; p++) {
                var visible = Plots[p].Visible(current.LocationX, current.LocationY);
                if (visible && !current.PlotsVisible[p]) {
                    Plots[p].ResetState(current.PlotStates[p], current.LocationX, current.LocationY);
                    current.PlotsVisible[p] = true;
                } else if (!visible && current.PlotsVisible[p]) {
                    current.PlotsVisible[p] = false;
                }
            }
        }

        public void ResetState(ContentState current, int lineNo) {
            current.LocationY = lineNo;
            current.LocationX = 0;
            for (int p = 0; p < Plots.Length; p++) {
                current.PlotsVisible[p] = false;
                Plots[p].ResetState(current.PlotStates[p], 0, lineNo);
            }
        }


        public int Get(ContentState current) {
            int sum = -1;
            for (int p = 0; p < Plots.Length; p++) {
                if (current.PlotsVisible[p]) {
                    if (sum == -1) {
                        sum = Plots[p].GetNext(current.PlotStates[p]);
                    } else {
                        sum = Blend(sum, Plots[p].GetNext(current.PlotStates[p]), 50);
                    }
                }
            }
            return sum;
        }
        // !current.Plot1Visible && !current.Plot2Visible ? 0
        // : !current.Plot1Visible ? Plot2.GetNext(current.Plot2State)
        // : !current.Plot2Visible ? Plot1.GetNext(current.Plot1State)
        // : Blend(Plot1.GetNext(current.Plot1State), Plot2.GetNext(current.Plot2State), 50);

        private int Blend(int intensityA, int intensityB, int alpha) =>
            intensityA + ((intensityB - intensityA) * alpha) / 0xFF;
    }
}

