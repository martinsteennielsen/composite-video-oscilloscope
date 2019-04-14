namespace CompositeVideoOscilloscope {

    public struct View {
        public readonly ScreenResolution Resolution;
        public readonly (double MinX, double MaxX, double MinY, double MaxY, double Width, double Height) Out;
        public readonly (double dX, double dY) Scaler;

        public View(double minX, double maxX, double minY, double maxY, ScreenResolution resolution) {
            Resolution = resolution;
            Out = (minX, maxX, minY, maxY, maxX - minX, maxY - minY);
            Scaler = (Out.Width / Resolution.Width, Out.Height / Resolution.Height);
        }

        public (double outX, double outY) Transform(double inX, double inY) => (inX * Scaler.dX + Out.MinX, inY * Scaler.dY + Out.MinY);
    }

}
