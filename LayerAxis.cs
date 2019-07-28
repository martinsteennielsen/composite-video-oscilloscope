using System;


namespace CompositeVideoOscilloscope {

    public class LayerAxis : IScreenContent {
        readonly ViewPort View;
        readonly double GridPercentage;
        readonly double dX, dY;

        public LayerAxis(ScreenResolution resolution, double gridPercentage) {
            View = new ViewPort(0,0, resolution.Width, resolution.Height).SetView(0,0,100,100);
            GridPercentage = gridPercentage;
            dX = View.Transform(1,0).x - View.Transform(0,0).x;
            dY = View.Transform(0,1).y - View.Transform(0,0).y;
        }

        public int PixelValue(int x, int y) => Value( View.Transform(x,y) );

        public void VSync() { }

        private int  Value( (double x, double y) position) {
            if (Math.Abs(position.x % GridPercentage) < dX) { return 0; }
            if (Math.Abs(position.y % GridPercentage) < dY) { return 0; }
            return 255;
        }
    }
}
