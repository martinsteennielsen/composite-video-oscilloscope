using System;
using System.Collections.Generic;
using System.Linq;

namespace CompositeVideoOscilloscope {
    using static VideoStandard;

    public class VideoFrame {
        private readonly VideoStandard Standard;
        private readonly FrameContent Content;
        private readonly long SampleTimePs;

        public VideoFrame(VideoStandard standard, FrameContent content) {
            Standard = standard;
            Content = content;
            SampleTimePs = (long)((long)1e12 * Standard.Timing.DotTime);
        }

        public byte[] Get() =>
            GetLines().SelectMany(x => x).ToArray();

        private IEnumerable<List<byte>> GetLines() {
            var currentLine = new LineIterator { LineNumber = Standard.LineBlocks[0].sy };
            while (!currentLine.Finished) {
                yield return GetNextLine(currentLine);
            }
        }

        List<byte> GetNextLine(LineIterator currentLine) {
            var output = new List<byte>(320);
            var lineSegments = Standard.LineBlocks[currentLine.LineBlockCount].LineSegments;
            while (!currentLine.CurrentPixel.Finished) {
                output.Add(GetNextPixel(currentLine.CurrentPixel, lineSegments));
            }
            currentLine.LineNumber += Standard.LineBlocks[currentLine.LineBlockCount].dy;
            currentLine.LineCnt++;
            ResetPixels(currentLine.CurrentPixel, currentLine.LineNumber);
            if (currentLine.LineCnt >= Standard.LineBlocks[currentLine.LineBlockCount].Count) {
                currentLine.LineCnt = 0;
                currentLine.LineBlockCount++;
            }
            currentLine.Finished = !(currentLine.LineBlockCount < Standard.LineBlocks.Length && currentLine.LineCnt < Standard.LineBlocks[currentLine.LineBlockCount].Count);
            return output;
        }

        void ResetPixels(PixelIterator iter, int lineNo) {
            iter.Finished = false;
            iter.CurrentTimePs = 0;
            iter.LineSegmentCnt = 0;
            Content.Reset(iter.CurrentContent, lineNo);
        }

        byte GetNextPixel(PixelIterator currentPixel, LineSegment[] lineSegments) {
            var lineSegment = lineSegments[currentPixel.LineSegmentCnt];

            currentPixel.CurrentTimePs += SampleTimePs;
            if (lineSegment.Value == 255) {
                Content.Next(currentPixel.CurrentContent);
            }
            if (currentPixel.CurrentTimePs >= lineSegment.Duration) {
                currentPixel.CurrentTimePs -= lineSegment.Duration;
                currentPixel.LineSegmentCnt++;
                currentPixel.Finished = !(currentPixel.LineSegmentCnt < lineSegments.Length && currentPixel.CurrentTimePs < lineSegments[currentPixel.LineSegmentCnt].Duration);
            }

            return lineSegment.Value == 255 
                ? ToVideoLevel(contentValue: Content.Get(currentPixel.CurrentContent)) 
                : lineSegment.Value;
        }

        byte ToVideoLevel(int contentValue) {
            int vres = contentValue * (255 - Standard.BlackLevel);
            vres /= 255;
            vres += Standard.BlackLevel;
            vres = vres > 255 ? 255 : Math.Max(Standard.BlackLevel, vres);
            return contentValue == 0 ? (byte)Standard.BlackLevel : (byte)vres;
        }
    }
}
