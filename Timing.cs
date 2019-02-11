namespace CompositeVideoOscilloscope {
    public class Timing {
        public readonly double HFreq;
        public readonly double VFreq;
        public readonly double BandwidthFreq;
        public readonly double DotTime;
        public readonly double FrameTime;
        public readonly double LineTime;

        public Timing(double hFreq, double vFreq, double bandwidthFreq) {
            BandwidthFreq = bandwidthFreq;
            VFreq = vFreq;
            HFreq = hFreq;
            DotTime = 1.0 / (bandwidthFreq);
            FrameTime = 1.0 / vFreq;
            LineTime = 1.0 / (hFreq);
        }
    }

    public class PalTiming : Timing {
        public PalTiming() : base(hFreq: 15625, vFreq: 50, bandwidthFreq: 5e6) {
        }
    }
}