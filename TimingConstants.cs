namespace CompositeVideoOscilloscope {

    public struct SyncConstants {
        public double LineBlankingTime;
        public double LineSyncTime;
        public double FrontPorchTime;
        public double EquPulseTime;
        public double VerticalSerrationTime;
        public int BlackLevel;
        public static SyncConstants Pal => new SyncConstants { LineBlankingTime = 12.05e-6, LineSyncTime = 4.7e-6, FrontPorchTime = 1.65e-6, EquPulseTime = 2.3e-6, VerticalSerrationTime = 4.7e-6, BlackLevel = (int)(255*0.3) };
    }


    public class TimingConstants {
        public readonly double HFreq;
        public readonly double VFreq;
        public readonly double BandwidthFreq;
        public readonly double DotTime;
        public readonly double FrameTime;
        public readonly double LineTime;
        public readonly SyncConstants SyncTimes;

        public TimingConstants(double hFreq, double vFreq, double bandwidthFreq, SyncConstants syncTimes) {
            BandwidthFreq = bandwidthFreq;
            VFreq = vFreq;
            HFreq = hFreq;
            DotTime = 1d / (bandwidthFreq);
            FrameTime = 1.0 / vFreq;
            LineTime = 1.0 / (hFreq);
            SyncTimes = syncTimes;
        }
    }

    public class PalTiming : TimingConstants {
        public PalTiming() : base(hFreq: 15625, vFreq: 50, bandwidthFreq: 5e6, syncTimes: SyncConstants.Pal) {
        }
    }
}
