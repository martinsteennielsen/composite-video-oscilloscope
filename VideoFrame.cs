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
            var currentLine = new LineState { LineNumber = Standard.LineBlocks[0].sy };
            while (!currentLine.Finished) {
                yield return GetNextLine(currentLine);
            }
        }

        List<byte> GetNextLine(LineState currentLine) {
            var output = new List<byte>(320);
            var lineSegments = Standard.LineBlocks[currentLine.LineBlockCount].LineSegments;
            while (!currentLine.PixelState.Finished) {
                output.Add(GetNextPixel(currentLine.PixelState, lineSegments));
            }
            currentLine.LineNumber += Standard.LineBlocks[currentLine.LineBlockCount].dy;
            currentLine.LineCnt++;
            ResetPixelState(currentLine.PixelState, currentLine.LineNumber);
            if (currentLine.LineCnt >= Standard.LineBlocks[currentLine.LineBlockCount].Count) {
                currentLine.LineCnt = 0;
                currentLine.LineBlockCount++;
            }
            currentLine.Finished = !(currentLine.LineBlockCount < Standard.LineBlocks.Length && currentLine.LineCnt < Standard.LineBlocks[currentLine.LineBlockCount].Count);
            return output;
        }

        void ResetPixelState(PixelState current, int lineNo) {
            current.Finished = false;
            current.TimePs = 0;
            current.LineSegmentCnt = 0;
            Content.ResetState(current.ContentState, lineNo);
        }

        byte GetNextPixel(PixelState current, LineSegment[] lineSegments) {
            var lineSegment = lineSegments[current.LineSegmentCnt];

            current.TimePs += SampleTimePs;
            if (lineSegment.Value == 255) {
                Content.Next(current.ContentState);
            }
            if (current.TimePs >= lineSegment.Duration) {
                current.TimePs -= lineSegment.Duration;
                current.LineSegmentCnt++;
                current.Finished = !(current.LineSegmentCnt < lineSegments.Length && current.TimePs < lineSegments[current.LineSegmentCnt].Duration);
            }

            return lineSegment.Value == 255 
                ? ToVideoLevel(contentValue: Content.Get(current.ContentState)) 
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
