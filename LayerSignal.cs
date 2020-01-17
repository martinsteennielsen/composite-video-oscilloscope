using System;
using static CompositeVideoOscilloscope.Sampling;

namespace CompositeVideoOscilloscope {

    public class LayerSignal {
        private readonly Viewport View;
        private readonly SubSampleIterator Iterator;

        public LayerSignal(Viewport screen, Sampling sample, PlotControls controls, double angle, VideoStandard standard) {
            var divisionsPrQuadrant = controls.NumberOfDivisions / 2;

            var triggerTimeNs = sample.RunTrigger(controls.Trigger);

            View = screen.SetView(
                1e9 * controls.Position.Time + triggerTimeNs,
                1e6*controls.Position.Voltage + 1e6*controls.Units.Voltage * divisionsPrQuadrant,
                triggerTimeNs + 1e9*controls.Position.Time + 1e9 * controls.Units.Time * controls.NumberOfDivisions,
                1e6*controls.Position.Voltage - 1e6*controls.Units.Voltage * divisionsPrQuadrant, angle);

            var delta = Subtract(View.TransformD(1, 0), View.TransformD(0, 0));
            Iterator = new SubSampleIterator(View, sample, delta);
        }

        (int, int) Subtract((int, int) a, (int, int) b) =>
            (a.Item1 - b.Item1, a.Item2 - b.Item2);

        public void Next() =>
            Iterator.Next();

        public void Start(int startX, int startY) =>
            Iterator.Start(startX,startY);

        public int Get() =>
            Iterator.Get() << 5;

        class SubSampleIterator {
            private readonly Viewport View;
            private readonly Sampling Sample;
            private readonly (int, int) Delta;

            public SubSampleIterator(Viewport view, Sampling sample, (int, int) delta) {
                View = view;
                Sample = sample;
                Delta = delta;
            }

            public void Start(int startX, int startY) {
                iterB = Sample.CreateIterator(start: View.TransformD(startX+0.5, startY - 1), delta: Delta);
                iterF = Sample.CreateIterator(start: View.TransformD(startX+0.5, startY - 0.5), delta: Delta);
                iterI = Sample.CreateIterator(start: View.TransformD(startX+0.5, startY), delta: Delta);
                iterM = Sample.CreateIterator(start: View.TransformD(startX+0.5, startY + 0.5), delta: Delta);
                iterP = Sample.CreateIterator(start: View.TransformD(startX+0.5, startY + 1), delta: Delta);
                iterG = Sample.CreateIterator(start: View.TransformD(startX+1,   startY - 0.5), delta: Delta);
                iterN = Sample.CreateIterator(start: View.TransformD(startX+1,   startY + 0.5), delta: Delta);
                a=b=c=d=e=f=g=h=i=j=k=l=m=n=o=p=8;
            }

            Iterator iterB, iterF, iterG, iterI, iterM, iterN, iterP;
            int a, b, c, d, e, f, g, h, i, j, k, l, m, n, o, p;

            // 
            //    a   b
            //  c d e f g
            //    h X i  
            //  j k l m n
            //    o   p  
            // 
            public void Next() {
                a = b;
                c = e; d = f; e = g;
                h = i;
                j = l; k = m; l = n;
                o = p;
                b = Sample.GetNext(iterB);
                f = Sample.GetNext(iterF); 
                g = Sample.GetNext(iterG);
                i = Sample.GetNext(iterI);
                m = Sample.GetNext(iterM); 
                n = Sample.GetNext(iterN);
                p = Sample.GetNext(iterP);
            }

            public int Get() =>
                Pixel(a + c + h + e) + 
                Pixel(e + b + i + g) + 
                Pixel(j + h + l + o) + 
                Pixel(l + n + i + p) + 
                (Pixel(d + f + k + m) << 2);

            private byte Pixel(int sumDelta) =>
                sumDelta > 3 || sumDelta == -4 ? (byte)0 : (byte)1;

        }
    }
}
