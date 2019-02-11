using System;
using System.Collections.Generic;
using System.Text;

namespace CompositeVideoOscilloscope {
    public class CompositeSignal {
        readonly Timing Timing;

        public CompositeSignal(Timing timing) {
            Timing = timing;
        }

        public double Get(double time) => 0;
    }
}
