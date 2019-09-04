﻿using System;

namespace CompositeVideoOscilloscope {

    public class Viewport {
        readonly IClip Clip;
        readonly double[] Matrix;
        public readonly double Top, Left, Bottom, Right, Width, Height, CenterX, CenterY, Aspect;
        
        public Viewport(double left, double top, double right, double bottom, IClip clip = null, double[] matrix = null) {
            Top = top; Left=left; Bottom = bottom; Right=right;
            Width = Right - Left; Height = Bottom-Top;
            CenterX = Left + Width/2;
            CenterY = Top + Height/2;
            Aspect = Width / Height;
            Matrix = matrix ?? Scale(1,1); 
            Clip = clip ?? new ClipBox(left, top, right, bottom);
        }

        public Viewport SetView(double viewLeft, double viewTop, double viewRight, double viewBottom, double angle=0) {
            double width = viewRight - viewLeft, height = viewBottom - viewTop;
            var newMtx = Matrix;

            newMtx = Multiply(Translate(-Left, -Top), newMtx);
            newMtx = Multiply(Translate(-Width/2, -Height/2), newMtx);
            newMtx = Multiply(Scale(1/Aspect, 1), newMtx);
            newMtx = Multiply(Rotate(angle), newMtx);
            newMtx = Multiply(Scale(Aspect, 1), newMtx);
            newMtx = Multiply(Translate(Width/2, Height/2), newMtx);
            newMtx = Multiply(Scale(width/Width, height/Height), newMtx);
            newMtx = Multiply(Translate(viewLeft, viewTop), newMtx);

            return new Viewport(viewLeft, viewTop, viewRight, viewBottom, Clip, newMtx);
        }

        public bool Visible(double x, double y) =>
            Clip.Visible(x,y);
            
        public (double x, double y) Transform(double x, double y) =>
            ( x*Matrix[0] + y*Matrix[1] + Matrix[2], x*Matrix[3] + y*Matrix[4] + Matrix[5]  );

        public (double x, double y) Transform((double x , double y) pos)  =>
            Transform(pos.x, pos.y);

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
