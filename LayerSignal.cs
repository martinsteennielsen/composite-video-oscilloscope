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

            var delta = Sub(View.TransformD(1, 0), View.TransformD(0, 0));
            Iterator = new SubSampleIterator(View, sample, delta);
        }

        (int, int) Sub((int, int) a, (int, int) b) =>
            (a.Item1 - b.Item1, a.Item2 - b.Item2);

        public void Next() =>
            Iterator.GetNext();

        public void NewLine(int lineNo) =>
            Iterator.NewLine(lineNo);

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

            public int Get() =>
                Pixel(a + c + h + e) + 
                Pixel(e + b + i + g) + 
                Pixel(j + h + l + o) + 
                Pixel(l + n + i + p) + 
                (Pixel(d + f + k + m) << 2);

            private byte Pixel(int sumDelta) =>
                sumDelta > 3 || sumDelta == -4 ? (byte)0 : (byte)1;

            // 
            //    a   b
            //  c d e f g
            //    h   i  
            //  j k l m n
            //    o   p  
            // 

            Iteration bI, fI, gI, iI, mI, nI, pI;
            int a, b, c, d, e, f, g, h, i, j, k, l, m, n, o, p;

            public void NewLine(int lineNo) {
                bI = Sample.StartIteration(View.TransformD(0.5, lineNo - 1), Delta);
                fI = Sample.StartIteration(View.TransformD(0.5, lineNo - 0.5), Delta);
                iI = Sample.StartIteration(View.TransformD(0.5, lineNo), Delta);
                mI = Sample.StartIteration(View.TransformD(0.5, lineNo + .5), Delta);
                pI = Sample.StartIteration(View.TransformD(0.5, lineNo + 1), Delta);
                gI = Sample.StartIteration(View.TransformD(1, lineNo - 0.5), Delta);
                nI = Sample.StartIteration(View.TransformD(1, lineNo + 0.5), Delta);
            }

            public void GetNext() {
                a = b;
                c = e; d = f; e = g;
                h = i;
                j = l; k = m; l = n;
                o = p;
                b = Sample.GetNext(bI);
                f = Sample.GetNext(fI); 
                g = Sample.GetNext(gI);
                i = Sample.GetNext(iI);
                m = Sample.GetNext(mI); 
                n = Sample.GetNext(nI);
                p = Sample.GetNext(pI);
            }
        }
    }
}
