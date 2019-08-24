namespace CompositeVideoOscilloscope {

    public interface IClip {
        bool Visible(double x, double y);
    }

    public class ClipBox : IClip {
        readonly double Top, Left, Bottom, Right;
   
        public ClipBox(double left, double top, double right, double bottom) {
            Top = top; Left=left; Bottom = bottom; Right=right;
        }

        public bool Visible(double x, double y) =>
            y>=Top && y<=Bottom && x >= Left && x <= Right;
    }

    public class ClipCircle : IClip {
        readonly double RadiusXSquared, RadiusYSquared, CenterX, CenterY;
   
        public ClipCircle(double centerX, double centerY, double radiusX, double radiusY) {
            CenterX =centerX;
            CenterY = centerY;
            RadiusXSquared=radiusX*radiusX;
            RadiusYSquared=radiusY*radiusY;
        }

        public bool Visible(double x, double y) =>
            ((x-CenterX)*(x-CenterX)/RadiusXSquared) + ((y-CenterY)*(y-CenterY) / RadiusYSquared)  < 1;
    }
}