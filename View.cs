namespace CompositeVideoOscilloscope {

    public struct View {
        public readonly (double MinX, double MaxX, double MinY, double MaxY, double Width, double Height) Out;
        public readonly (double Width, double Height) In;
        public readonly (double dX, double dY) Scaler;

        public View(TimingConstants timing, double minX, double maxX, double minY, double maxY) {
            Out = (minX, maxX, minY, maxY, maxX - minX, maxY - minY );
            In = (timing.BandwidthFreq / timing.HFreq,  2d * timing.HFreq / timing.VFreq);
            Scaler = (Out.Width / In.Width, Out.Height / In.Height);
        }
        public (double outX, double outY) Scale(double inX, double inY) => (inX * Scaler.dX + Out.MinX, inY * Scaler.dY + Out.MinY);
    }

}
