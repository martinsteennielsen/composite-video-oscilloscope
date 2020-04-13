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

        public Content Content =>
            new Content(
                Controls.Plots.Select(ctrl => new SignalPlot(ctrl, VideoConstants.Get(Controls.VideoStandard), sampling: Aquisition.GetSampling(ctrl, Controls.CurrentTime)))
                );

    }

    public class Content {
        readonly SignalPlot[] Plots;

        public Content(IEnumerable<SignalPlot> plots) {
            Plots = plots.ToArray();
        }

        public int GetNext(ContentState current) {
            var value = Get(current);
            current.LocationX++;
            for (int p = 0; p < Plots.Length; p++) {
                var visible = Plots[p].Visible(current.LocationX, current.LocationY);
                if (visible && !current.PlotStates[p].Visible) {
                    Plots[p].ResetState(current.PlotStates[p], current.LocationX, current.LocationY);
                    current.PlotStates[p].Visible = true;
                } else if (!visible && current.PlotStates[p].Visible) {
                    current.PlotStates[p].Visible = false;
                }
            }
            return value;
        }

        public void ResetState(ContentState current, int lineNo) {
            current.LocationY = lineNo;
            current.LocationX = 0;
            for (int p = 0; p < Plots.Length; p++) {
                Plots[p].ResetState(current.PlotStates[p], 0, lineNo);
            }
        }

        int Get(ContentState current) {
            int sum = -1;
            for (int p = 0; p < Plots.Length; p++) {
                if (current.PlotStates[p].Visible) {
                    sum = sum == -1 ? get(p) : blend(sum, get(p), 50);
                }
            }
            return sum;

            int blend(int intensityA, int intensityB, int alpha) =>
                intensityA + (intensityB - intensityA) * alpha / 0xFF;

            int get(int p) =>
                Plots[p].GetNext(current.PlotStates[p]);
        }
    }
}

