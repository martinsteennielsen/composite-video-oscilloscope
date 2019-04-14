using System;


namespace CompositeVideoOscilloscope {


    public class LayerAxis : Layer, ILayer {
        (double dx, double dy) Scale;

        public LayerAxis(TimingConstants timing) : base(timing, 0, 200, 0, 200) {
            Scale = View.Scale();
        }

        public double PixelValue(int x, int y, double currentValue) => currentValue * Value( View.ToView(x,y) );

        private double Value( (double x, double y) position) {
            if (Math.Abs(position.x % 20) < Scale.dx) { return 0; }
            if (Math.Abs(position.y % 20) < Scale.dy) { return 0; }
            return 1;
        }
    }
}
