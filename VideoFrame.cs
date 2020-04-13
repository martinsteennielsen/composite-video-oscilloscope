using System;
using System.Collections.Generic;
using System.Linq;

namespace CompositeVideoOscilloscope {
    using static VideoConstants;

    public class VideoFrame {
        private readonly VideoConstants VideoConstants;
        private readonly Content Content;
        private readonly long SampleTimePs;

        public VideoFrame(VideoConstants constants, Content content) {
            VideoConstants = constants;
            Content = content;
            SampleTimePs = (long)((long)1e12 * VideoConstants.Timing.DotTime);
        }

        public byte[] Get() =>
            GetLines().SelectMany(x => x).ToArray();

        private IEnumerable<List<byte>> GetLines() {
            var currentLine = new LineState();
            while (!currentLine.Finished) {
                ResetPixelState(currentLine.PixelState, currentLine.LineNumber);
                yield return GetNextLine(currentLine);

            }
        }

        List<byte> GetNextLine(LineState currentLine) {
            var output = new List<byte>(320);
            var lineSegments = VideoConstants.LineBlocks[currentLine.LineBlockCount].LineSegments;
            while (!currentLine.PixelState.Finished) {
                output.Add(GetNextPixel(currentLine.PixelState, lineSegments));
            }
            currentLine.LineNumber += VideoConstants.LineBlocks[currentLine.LineBlockCount].dy;
            currentLine.LineCnt++;
            if (currentLine.LineCnt >= VideoConstants.LineBlocks[currentLine.LineBlockCount].Count) {
                currentLine.LineCnt = 0;
                currentLine.LineBlockCount++;
                currentLine.Finished = !(currentLine.LineBlockCount < VideoConstants.LineBlocks.Length && currentLine.LineCnt < VideoConstants.LineBlocks[currentLine.LineBlockCount].Count);
                if (!currentLine.Finished) {
                    currentLine.LineNumber = VideoConstants.LineBlocks[currentLine.LineBlockCount].sy;
                }
            }
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

            var pixelValue = lineSegment.Value == 255
                ? ToVideoLevel(contentValue: Content.GetNext(current.ContentState))
                : lineSegment.Value;

            current.TimePs += SampleTimePs;
            if (current.TimePs >= lineSegment.Duration) {
                current.TimePs -= lineSegment.Duration;
                current.LineSegmentCnt++;
                current.Finished = !(current.LineSegmentCnt < lineSegments.Length && current.TimePs < lineSegments[current.LineSegmentCnt].Duration);
            }

            return pixelValue;
        }

        byte ToVideoLevel(int contentValue) {
            int vres = contentValue * (255 - VideoConstants.BlackLevel);
            vres /= 255;
            vres += VideoConstants.BlackLevel;
            vres = vres > 255 ? 255 : Math.Max(VideoConstants.BlackLevel, vres);
            return contentValue == 0 ? (byte)VideoConstants.BlackLevel : (byte)vres;
        }
    }
}
