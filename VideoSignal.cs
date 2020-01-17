using System;
using System.Collections.Generic;
using static CompositeVideoOscilloscope.Screen;
using static CompositeVideoOscilloscope.VideoStandard;

namespace CompositeVideoOscilloscope {

    public interface IContent {
        int Get();
        void Next();
        void Start(int currentLine);
    }

    public class VideoSignal {
        public IEnumerable<IEnumerable<byte>> GenerateFrame(VideoStandard standard, IContent content) {
            var iterator = new LineIterator(standard, content);
            while (!iterator.Completed) {
                yield return iterator.Get();
                iterator.Next();
            }
        }

        class LineIterator {
            const long ps = (long)1e12;

            private readonly IContent Content;
            private readonly VideoStandard Standard;

            private int LineBlockCount = 0;
            private int LineCnt = 0;
            private int CurrentLine;

            public LineIterator(VideoStandard standard, IContent content) {
                Standard = standard;
                CurrentLine = Standard.LineBlocks[LineBlockCount].sy;
                Content = content;
                LineBlockCount = 0;
            }

            public bool Completed =>
                !(LineBlockCount < Standard.LineBlocks.Length && LineCnt < Standard.LineBlocks[LineBlockCount].Count);

            public List<byte> Get() {
                var lineSegments = Standard.LineBlocks[LineBlockCount].LineSegments;
                var pixelIterator = new PixelIterator(Content, lineSegments, (long)(ps * Standard.Timing.DotTime), Standard.BlackLevel, CurrentLine);

                var output = new List<byte>(320);
                while (!pixelIterator.Completed) {
                    output.Add(pixelIterator.Get());
                    pixelIterator.Next();
                }
                return output;
            }

            public void Next() {
                CurrentLine += Standard.LineBlocks[LineBlockCount].dy;
                LineCnt++;
                if (LineCnt >= Standard.LineBlocks[LineBlockCount].Count) {
                    LineCnt = 0;
                    LineBlockCount++;
                }
            }
        }

        class PixelIterator {
            private readonly IContent Content;
            private readonly int BlackLevel;
            private readonly LineSegment[] LineSegments;
            private readonly long SampleTimePs;

            long CurrentTimePs;
            int LineSegmentCnt;

            public PixelIterator(IContent content, LineSegment[] lineSegments, long sampleTimePs, int blackLevel, int lineNumber) {
                BlackLevel = blackLevel;
                Content = content;
                Content.Start(lineNumber);
                LineSegments = lineSegments;

                LineSegmentCnt = 0;
                CurrentTimePs = 0;
                SampleTimePs = sampleTimePs;
            }

            public bool Completed =>
                !(LineSegmentCnt < LineSegments.Length && CurrentTimePs < LineSegments[LineSegmentCnt].Duration);

            public void Next() {
                CurrentTimePs += SampleTimePs;
                if (LineSegments[LineSegmentCnt].Value == 255) {
                    Content.Next();
                }
                if (CurrentTimePs >= LineSegments[LineSegmentCnt].Duration) {
                    CurrentTimePs -= LineSegments[LineSegmentCnt].Duration;
                    LineSegmentCnt++;
                }
            }

            public byte Get() {
                byte intensity() {
                    int val = Content.Get();
                    int vres = val * (255 - BlackLevel);
                    vres /= 255;
                    vres += BlackLevel;
                    vres = vres > 255 ? 255 : Math.Max(BlackLevel, vres);
                    return val == 0 ? (byte)BlackLevel : (byte)vres;
                }

                var value = LineSegments[LineSegmentCnt].Value;
                return value == 255 ?  intensity() : value;
            }
        }
    }
}