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

    public class Timing {
        public readonly double HFreq;
        public readonly double VFreq;
        public readonly double BandwidthFreq;
        public readonly double DotTime;
        public readonly double FrameTime;
        public readonly double LineTime;
        public readonly SyncConstants SyncTimes;
        public Timing(double hFreq, double vFreq, double bandwidthFreq, SyncConstants syncTimes) {
            BandwidthFreq = bandwidthFreq;
            VFreq = vFreq;
            HFreq = hFreq;
            DotTime = 1d / (bandwidthFreq);
            FrameTime = 1.0 / vFreq;
            LineTime = 1.0 / (hFreq);
            SyncTimes = syncTimes;
        }

        public static Timing Pal => new Timing(hFreq: 15625, vFreq: 50, bandwidthFreq: 5e6, syncTimes: SyncConstants.Pal);

    }

    public struct VideoStandard {
        const int ns = 10000000;
        public readonly SignalBlocks[] Blocks; 
        public readonly Timing Timing;

        private VideoStandard(SignalBlocks[] signals, Timing timing) {
            Blocks=signals;
            Timing = timing;
        }

        public static VideoStandard Pal5MhzInterlaced = new VideoStandard(signals: InterlacedFrame(Timing.Pal), timing: Timing.Pal);

        public double VisibleWidth => Timing.BandwidthFreq/Timing.HFreq - (Timing.SyncTimes.LineBlankingTime / Timing.DotTime);
        public double VisibleHeight =>  2 * Timing.HFreq / Timing.VFreq - 25;

        public int BlackLevel => Timing.SyncTimes.BlackLevel;

        public struct Signal { public int Value; public int Duration; };
        public struct SignalBlocks { public int Count; public Signal[] Signals; public int dy; public int sy; };
        

        private static SignalBlocks[] InterlacedFrame(Timing timing) {
            int dark = timing.SyncTimes.BlackLevel, sign = 255, sync = 0;

            var synl = new[] {
                new Signal { Value = sync , Duration = (int)(ns * (0.5 * timing.LineTime - timing.SyncTimes.LineSyncTime)) },
                new Signal { Value = dark , Duration = (int)(ns * timing.SyncTimes.LineSyncTime )} };
            var syns = new[] {
                new Signal { Value = dark , Duration = (int)(ns * (0.5 * timing.LineTime - timing.SyncTimes.EquPulseTime)) },
                new Signal { Value = sync,  Duration = (int)(ns * timing.SyncTimes.EquPulseTime) } };
            var line = new[] {
                new Signal { Value = sync , Duration = (int)(ns * timing.SyncTimes.LineSyncTime) },
                new Signal { Value = dark , Duration = (int)(ns * (timing.SyncTimes.LineBlankingTime - timing.SyncTimes.FrontPorchTime - timing.SyncTimes.LineSyncTime)) },
                new Signal { Value = sign , Duration = (int)(ns * (timing.LineTime - timing.SyncTimes.LineBlankingTime)) },
                new Signal { Value = dark , Duration = (int)(ns * timing.SyncTimes.FrontPorchTime) },
                };
            var blank = new[] {
                new Signal { Value = sync , Duration = (int)(ns * timing.SyncTimes.LineSyncTime) },
                new Signal { Value = dark , Duration = (int)(ns * (timing.LineTime - timing.SyncTimes.LineSyncTime)) },
                };

            return new[] {
                new SignalBlocks { Count = 5  , Signals = synl }, new SignalBlocks { Count = 5  , Signals = syns },
                new SignalBlocks { Count = 12 , Signals = blank },
                new SignalBlocks { Count = 293, Signals = line, dy = 2, sy = 1 },
                new SignalBlocks { Count = 5  , Signals = syns }, new SignalBlocks { Count = 5  , Signals = synl }, new SignalBlocks { Count = 4  , Signals = syns },
                new SignalBlocks { Count = 12 , Signals = blank },
                new SignalBlocks { Count = 293, Signals = line, dy = 2, sy = 0 },
                new SignalBlocks { Count = 6  , Signals = syns },
            };
        }

    }
}
