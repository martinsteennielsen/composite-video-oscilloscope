namespace CompositeVideoOscilloscope {

    public struct ScreenResolution {
        public readonly int Width, Height;

        public ScreenResolution(TimingConstants timing) {
            Width = (int) (timing.BandwidthFreq / timing.HFreq);
            Height = (int) ( 2d * timing.HFreq / timing.VFreq);
        }

        public ScreenResolution(int width, int height) {
            Width = width;
            Height = height;
        }
    }
}