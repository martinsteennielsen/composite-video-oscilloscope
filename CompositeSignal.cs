using System;

namespace CompositeVideoOscilloscope {
    public class CompositeSignal {
        readonly Timing Timing;
        readonly Random Randomizer = new Random();

        public CompositeSignal(Timing timing) {
            Timing = timing;
        }
        
        public double Get(double time) => Randomizer.NextDouble();
    }
}
