using System;

namespace CompositeVideoOscilloscope {


    public class ScreenContent {
        readonly SignalContent SignalContent;
        
        public ScreenContent(Controls controls, InputSignal signal) {
            SignalContent = new SignalContent(controls, signal);
        }

        public int Intensity(int x, int y) =>
            SignalContent.Visible(x,y) ? SignalContent.Intensity(x,y) : 0;
    }
}
