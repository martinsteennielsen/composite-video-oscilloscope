namespace CompositeVideoOscilloscope {

    public struct Viewport {
        public readonly double[] Matrix;
        public readonly double Top, Left, Bottom, Right, Width, Height;
        
        public Viewport(double left, double top, double right, double bottom, double[] matrix = null) {
            Top = top; Left=left; Bottom = bottom; Right=right;
            Width = Right - Left; Height = Bottom-Top;
            Matrix = matrix ?? Scale(1,1); 
        }

        public Viewport SetView(double viewLeft, double viewTop, double viewRight, double viewBottom) {
            double sx = (viewRight-viewLeft)/(Right-Left), sy = (viewBottom-viewTop)/(Bottom-Top);
            return new Viewport(Left, Top, Right, Bottom, Multiply ( Translate(viewLeft,viewTop), Scale(sx,sy) ) );
        }
        public bool Visible(double x, double y) =>
            y>=Top && y<=Bottom && x >= Left && x <= Right;
            
        public (double x, double y) Transform(double x, double y) =>
            ( x*Matrix[0] + y * Matrix[1] + Matrix[2], x * Matrix[3] + y*Matrix[4] + Matrix[5]  );

        static double[] Scale(double sx, double sy) => 
            new double[] { sx, 0, 0,  0, sy, 0,  0, 0, 1 };

        static double[] Translate(double tx, double ty) => 
            new double[] { 1, 0, tx, 0, 1, ty, 0,0,1 };

        static double[] Multiply(double[] m1, double[] m2) => 
             new [] { 
                m1[0]*m2[0] + m1[1]*m2[3] + m1[2] * m2[6],  
                m1[0]*m2[1] + m1[1]*m2[4] + m1[2] * m2[7],  
                m1[0]*m2[2] + m1[1]*m2[5] + m1[2] * m2[8],  
                m1[3]*m2[0] + m1[4]*m2[3] + m1[5] * m2[6],  
                m1[3]*m2[1] + m1[4]*m2[4] + m1[5] * m2[7],  
                m1[3]*m2[2] + m1[4]*m2[5] + m1[5] * m2[8],  
                m1[6]*m2[0] + m1[7]*m2[3] + m1[8] * m2[6],  
                m1[6]*m2[1] + m1[7]*m2[4] + m1[8] * m2[7],  
                m1[6]*m2[2] + m1[7]*m2[5] + m1[8] * m2[8],  
            };

    }
}
