using System;
using System.Collections.Generic;

namespace CompositeVideoOscilloscope {

    public interface IContent {
        int Intensity(int x, int y);
    }

    public class VideoSignal {
        public IEnumerable<byte[]> GenerateFrame(VideoStandard standard, IContent content) {
            var iterator = new LineIterator(standard, content);
            while (iterator.HasNext()) {
                yield return iterator.GetNext();
            }
        }

        class LineIterator {
            private readonly IContent Content;
            private readonly VideoStandard Standard;

            private int LineBlockCount = 0;
            private int LineCnt = 0;
            private int CurrentLine;

            public LineIterator(VideoStandard standard, IContent content) {
                Standard = standard;
                Content = content;
                LineBlockCount = 0;
                CurrentLine = Standard.LineBlocks[LineBlockCount].sy;
            }

            public bool HasNext() =>
                LineBlockCount < Standard.LineBlocks.Length && LineCnt < Standard.LineBlocks[LineBlockCount].Count;

            public byte[] GetNext() {
                var output = Get();
                Next();
                return output;
            }

            private byte[] Get() {
                var line = Standard.LineBlocks[LineBlockCount].LineSegments;
                var pixelIterator = new PixelIterator(Content, line, CurrentLine, (long)(Standard.Timing.DotTime * 1e12), Standard.BlackLevel);

                var output = new List<byte>();
                while (pixelIterator.HasNext()) {
                    output.Add(pixelIterator.GetNext());
                }

                return output.ToArray();
            }

            private void Next() {
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
            private readonly VideoStandard _Standard;
            private readonly VideoStandard.LineSegment[] Line;
            private readonly int LineNumber;
            private readonly long SampleTimePs;

            long CurrentTimePs;
            int LineSegmentCnt;
            int xPos;

            public PixelIterator(IContent content, VideoStandard.LineSegment[] line, int lineNumber, long sampleTimePs, int blackLevel) {
                BlackLevel = blackLevel;
                Content = content;
                Line = line;
                LineNumber = lineNumber;

                LineSegmentCnt = 0;
                CurrentTimePs = 0;
                SampleTimePs = sampleTimePs;
                xPos = 0;
            }

            public bool HasNext() =>
                LineSegmentCnt < Line.Length && CurrentTimePs < Line[LineSegmentCnt].Duration;

            public byte GetNext() {
                byte value = Get();
                Next();
                return value;
            }

            private void Next() {
                CurrentTimePs += SampleTimePs;
                if (CurrentTimePs >= Line[LineSegmentCnt].Duration) {
                    CurrentTimePs -= Line[LineSegmentCnt].Duration;
                    LineSegmentCnt++;
                }
                xPos++;
            }

            private byte Get() {
                byte intensity(int sx, int sy) {
                    int val = Content.Intensity(sx, sy);
                    int vres = val * (255 - BlackLevel);
                    vres /= 255;
                    vres += BlackLevel;
                    vres = vres > 255 ? 255 : Math.Max(BlackLevel, vres);
                    return val == 0 ? (byte)BlackLevel : (byte)vres;
                }

                var value = Line[LineSegmentCnt].Value;
                return value == 255 ?  intensity(xPos, LineNumber) : value;
            }
        }
    }
}