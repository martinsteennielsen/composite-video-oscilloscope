namespace CompositeVideoOscilloscope {

    public struct SyncConstants {
        public double LineBlankingTime;
        public double LineSyncTime;
        public double FrontPorchTime;
        public double EquPulseTime;
        public double VerticalSerrationTime;
        public byte BlackLevel;
        public int BlankLines;
        public static SyncConstants Pal => new SyncConstants { LineBlankingTime = 12.05e-6, LineSyncTime = 4.7e-6, FrontPorchTime = 1.65e-6, EquPulseTime = 2.3e-6, VerticalSerrationTime = 4.7e-6, BlackLevel = (int)(255 * 0.3), BlankLines = 38 };
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

        public static Timing iPal => new Timing(hFreq: 15625, vFreq: 50, bandwidthFreq: 5e6, syncTimes: SyncConstants.Pal);
        public static Timing pPal => new Timing(hFreq: 15625, vFreq: 25, bandwidthFreq: 5e6, syncTimes: SyncConstants.Pal);
        public static Timing pPal10 => new Timing(hFreq: 15625, vFreq: 25, bandwidthFreq: 10e6, syncTimes: SyncConstants.Pal);
    }

    public struct VideoStandard {
        const long ps = (long)1e12;
        public readonly LineBlock[] LineBlocks;
        public readonly Timing Timing;
        public readonly int InterlacedScaler;
        private VideoStandard(LineBlock[] lineBlocks, Timing timing, bool interlaced) {
            LineBlocks = lineBlocks;
            Timing = timing;
            InterlacedScaler = interlaced ? 2 : 1;
        }

        public static VideoStandard Pal5MhzInterlaced = new VideoStandard(lineBlocks: InterlacedFrame(Timing.iPal), timing: Timing.iPal, interlaced: true);
        public static VideoStandard Pal5MhzProgessiv = new VideoStandard(lineBlocks: ProgressiveFrame(Timing.pPal), timing: Timing.pPal, interlaced: false);
        public static VideoStandard Pal10MhzProgessiv = new VideoStandard(lineBlocks: ProgressiveFrame(Timing.pPal10), timing: Timing.pPal10, interlaced: false);

        public double VisibleWidth => Timing.BandwidthFreq / Timing.HFreq - (Timing.SyncTimes.LineBlankingTime / Timing.DotTime);
        public double VisibleHeight => InterlacedScaler * Timing.HFreq / Timing.VFreq - Timing.SyncTimes.BlankLines;

        public int BlackLevel => Timing.SyncTimes.BlackLevel;

        public struct LineSegment { public byte Value; public long Duration; };
        public struct LineBlock { public int Count; public LineSegment[] LineSegments; public int dy; public int sy; };

        private static LineBlock[] InterlacedFrame(Timing timing) {
            byte dark = timing.SyncTimes.BlackLevel, sign = 255, sync = 0;

            var synlLine = new[] {
                new LineSegment { Value = sync , Duration = (long)(ps * (0.5 * timing.LineTime - timing.SyncTimes.LineSyncTime)) },
                new LineSegment { Value = dark , Duration = (long)(ps * timing.SyncTimes.LineSyncTime )} };
            var synsLine = new[] {
                new LineSegment { Value = dark , Duration = (long)(ps * (0.5 * timing.LineTime - timing.SyncTimes.EquPulseTime)) },
                new LineSegment { Value = sync,  Duration = (long)(ps * timing.SyncTimes.EquPulseTime) } };
            var pictureLine = new[] {
                new LineSegment { Value = sync , Duration = (long)(ps * timing.SyncTimes.LineSyncTime) },
                new LineSegment { Value = dark , Duration = (long)(ps * (timing.SyncTimes.LineBlankingTime - timing.SyncTimes.FrontPorchTime - timing.SyncTimes.LineSyncTime)) },
                new LineSegment { Value = sign , Duration = (long)(ps * (timing.LineTime - timing.SyncTimes.LineBlankingTime)) },
                new LineSegment { Value = dark , Duration = (long)(ps * timing.SyncTimes.FrontPorchTime) },
                };
            var blankLine = new[] {
                new LineSegment { Value = sync , Duration = (long)(ps * timing.SyncTimes.LineSyncTime) },
                new LineSegment { Value = dark , Duration = (long)(ps * (timing.LineTime - timing.SyncTimes.LineSyncTime)) },
                };

            return new[] {
                new LineBlock { Count = 5  , LineSegments = synlLine }, new LineBlock { Count = 5  , LineSegments = synsLine },
                new LineBlock { Count = 12 , LineSegments = blankLine },
                new LineBlock { Count = 293, LineSegments = pictureLine, dy = 2, sy = 1 },
                new LineBlock { Count = 5  , LineSegments = synsLine }, new LineBlock { Count = 5  , LineSegments = synlLine }, new LineBlock { Count = 4  , LineSegments = synsLine },
                new LineBlock { Count = 12 , LineSegments = blankLine },
                new LineBlock { Count = 293, LineSegments = pictureLine, dy = 2, sy = 0 },
                new LineBlock { Count = 6  , LineSegments = synsLine },
            };
        }

        private static LineBlock[] ProgressiveFrame(Timing timing) {
            byte dark = timing.SyncTimes.BlackLevel, sign = 255, sync = 0;

            var synlLine = new[] {
                new LineSegment { Value = sync , Duration = (long)(ps * (0.5 * timing.LineTime - timing.SyncTimes.LineSyncTime)) },
                new LineSegment { Value = dark , Duration = (long)(ps * timing.SyncTimes.LineSyncTime )} };
            var synsLine = new[] {
                new LineSegment { Value = dark , Duration = (long)(ps * (0.5 * timing.LineTime - timing.SyncTimes.EquPulseTime)) },
                new LineSegment { Value = sync,  Duration = (long)(ps * timing.SyncTimes.EquPulseTime) } };
            var pictureLine = new[] {
                new LineSegment { Value = sync , Duration = (long)(ps * timing.SyncTimes.LineSyncTime) },
                new LineSegment { Value = dark , Duration = (long)(ps * (timing.SyncTimes.LineBlankingTime - timing.SyncTimes.FrontPorchTime - timing.SyncTimes.LineSyncTime)) },
                new LineSegment { Value = sign , Duration = (long)(ps * (timing.LineTime - timing.SyncTimes.LineBlankingTime)) },
                new LineSegment { Value = dark , Duration = (long)(ps * timing.SyncTimes.FrontPorchTime) },
                };
            var blankLine = new[] {
                new LineSegment { Value = sync , Duration = (long)(ps * timing.SyncTimes.LineSyncTime) },
                new LineSegment { Value = dark , Duration = (long)(ps * (timing.LineTime - timing.SyncTimes.LineSyncTime)) },
                };

            return new[] {
                new LineBlock { Count = 5  , LineSegments = synlLine }, new LineBlock { Count = 5  , LineSegments = synsLine },
                new LineBlock { Count = 12 , LineSegments = blankLine },
                new LineBlock { Count = 593, LineSegments = pictureLine, dy = 1, sy = 0 },
                new LineBlock { Count = 12 , LineSegments = blankLine },
                new LineBlock { Count = 6  , LineSegments = synsLine },
            };
        }
    }
}
