using System;

namespace CompositeVideoOscilloscope {

    public class Viewport {
        readonly IClip Clip;
        readonly double[] Transformation;
        public readonly double Top, Left, Bottom, Right, Width, Height, CenterX, CenterY, Aspect;
        
        public Viewport(double left, double top, double right, double bottom, IClip clip = null, double[] transformation = null) {
            Top = top; Left=left; Bottom = bottom; Right=right;
            Width = Right - Left; Height = Bottom-Top;
            CenterX = Left + Width/2;
            CenterY = Top + Height/2;
            Aspect = Width / Height;
            Transformation = transformation ?? Scale(1,1); 
            Clip = clip ?? new ClipBox(left, top, right, bottom);
        }

        public Viewport SetView(double viewLeft, double viewTop, double viewRight, double viewBottom, double angle=0) {
            double width = viewRight - viewLeft, height = viewBottom - viewTop;
            var newTransformation = Transformation;

            newTransformation = Multiply(Translate(-Left, -Top), newTransformation);
            newTransformation = Multiply(Translate(-Width/2, -Height/2), newTransformation);
            newTransformation = Multiply(Scale(1/Aspect, 1), newTransformation);
            newTransformation = Multiply(Rotate(angle), newTransformation);
            newTransformation = Multiply(Scale(Aspect, 1), newTransformation);
            newTransformation = Multiply(Translate(Width/2, Height/2), newTransformation);
            newTransformation = Multiply(Scale(width/Width, height/Height), newTransformation);
            newTransformation = Multiply(Translate(viewLeft, viewTop), newTransformation);

            return new Viewport(viewLeft, viewTop, viewRight, viewBottom, Clip, newTransformation);
        }

        public bool Visible(double x, double y) =>
            Clip.Visible(x,y);

        public (int x, int y) TransformI(int x, int y) =>
            ((int)(x * Transformation[0] + y * Transformation[1] + Transformation[2]),(int)(x * Transformation[3] + y * Transformation[4] + Transformation[5]));
        
        public (double x, double y) Transform(double x, double y) =>
            (x * Transformation[0] + y * Transformation[1] + Transformation[2], x * Transformation[3] + y * Transformation[4] + Transformation[5]);

        static double[] Scale(double sx, double sy) => 
            new double[] { sx, 0, 0,  0, sy, 0,  0, 0, 1 };

        static double[] Translate(double tx, double ty) =>
            new double[] { 1, 0, tx, 0, 1, ty, 0, 0, 1 };

        static double[] Rotate(double angle) =>
            new double[] { Math.Cos(angle), -Math.Sin(angle), 0, Math.Sin(angle), Math.Cos(angle), 0, 0, 0, 1 };

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
