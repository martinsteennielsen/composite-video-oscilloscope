using System;
using System.Collections.Generic;
using static CompositeVideoOscilloscope.Screen;
using static CompositeVideoOscilloscope.VideoStandard;

namespace CompositeVideoOscilloscope {

    using static FrameContentI;
    using static PixelsI;
    using static LinesI;

    public class VideoSignal {

        public IEnumerable<IEnumerable<byte>> GenerateFrame(VideoStandard standard, FrameContent content) {
            var lines = new Lines(standard, content);

            var iter = lines.Start();
            while (!lines.Completed(iter)) {
                yield return lines.Get(iter);
                lines.Next(iter);
            }
        }

    }

    public class LinesI {
        private int LineBlockCount = 0;
        private int LineCnt = 0;
        private int CurrentLine;
        private PixelsI PixelI;

        public class Lines {
            const long ps = (long)1e12;

            private readonly FrameContent Content;
            private readonly VideoStandard Standard;
            private readonly Pixels Pixels;

            public Lines(VideoStandard standard, FrameContent content) {
                Standard = standard;
                Content = content;
                Pixels = new Pixels(content, (long)(ps * Standard.Timing.DotTime), Standard.BlackLevel);
            }

            public LinesI Start() =>
                new LinesI { CurrentLine = Standard.LineBlocks[0].sy, LineCnt = 0, LineBlockCount = 0, PixelI = Pixels.Start(0, Standard.LineBlocks[0].LineSegments) };

            public bool Completed(LinesI iter) =>
                !(iter.LineBlockCount < Standard.LineBlocks.Length && iter.LineCnt < Standard.LineBlocks[iter.LineBlockCount].Count);

            public List<byte> Get(LinesI iter) {
                var output = new List<byte>(320);
                while (!Pixels.Completed(iter.PixelI)) {
                    output.Add(Pixels.Get(iter.PixelI));
                    Pixels.Next(iter.PixelI);
                }
                return output;
            }

            public void Next(LinesI iter) {
                iter.CurrentLine += Standard.LineBlocks[iter.LineBlockCount].dy;
                iter.LineCnt++;
                iter.PixelI = Pixels.Start(iter.CurrentLine, Standard.LineBlocks[iter.LineBlockCount].LineSegments);
                if (iter.LineCnt >= Standard.LineBlocks[iter.LineBlockCount].Count) {
                    iter.LineCnt = 0;
                    iter.LineBlockCount++;
                }
            }
        }
    }

    public class PixelsI {
        long CurrentTimePs;
        int LineSegmentCnt;
        FrameContentI ContentI;
        LineSegment[] LineSegments;

        public class Pixels {
            private readonly FrameContent Content;
            private readonly int BlackLevel;
            private readonly long SampleTimePs;

            public Pixels(FrameContent content, long sampleTimePs, int blackLevel) {
                BlackLevel = blackLevel;
                Content = content;
                SampleTimePs = sampleTimePs;
            }

            public PixelsI Start(int lineNo, LineSegment[] lineSegments) =>
                new PixelsI { LineSegments = lineSegments, CurrentTimePs = 0, LineSegmentCnt = 0, ContentI = Content.Start(lineNo) };

            public bool Completed(PixelsI iter) =>
                !(iter.LineSegmentCnt < iter.LineSegments.Length && iter.CurrentTimePs < iter.LineSegments[iter.LineSegmentCnt].Duration);

            public void Next(PixelsI iter) {
                iter.CurrentTimePs += SampleTimePs;
                if (iter.LineSegments[iter.LineSegmentCnt].Value == 255) {
                    Content.Next(iter.ContentI);
                }
                if (iter.CurrentTimePs >= iter.LineSegments[iter.LineSegmentCnt].Duration) {
                    iter.CurrentTimePs -= iter.LineSegments[iter.LineSegmentCnt].Duration;
                    iter.LineSegmentCnt++;
                }
            }

            public byte Get(PixelsI iter) {
                byte intensity() {
                    int val = Content.Get(iter.ContentI);
                    int vres = val * (255 - BlackLevel);
                    vres /= 255;
                    vres += BlackLevel;
                    vres = vres > 255 ? 255 : Math.Max(BlackLevel, vres);
                    return val == 0 ? (byte)BlackLevel : (byte)vres;
                }

                var value = iter.LineSegments[iter.LineSegmentCnt].Value;
                return value == 255 ? intensity() : value;
            }
        }
    }
}

