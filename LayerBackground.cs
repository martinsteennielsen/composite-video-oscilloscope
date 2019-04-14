namespace CompositeVideoOscilloscope {
    public class LayerBackground : ILayer {
        public double PixelValue(int x, int y, double currentValue) => 0.2;
    }
}
