using System;

namespace CompositeVideoOscilloscope {

    public class LayerSignal {
        private readonly Viewport View;
        private readonly SubSampleIterator Iterator;

        public LayerSignal(Viewport screen, Sampling sample, PlotControls controls, double angle, VideoStandard standard) {
            var divisionsPrQuadrant = controls.NumberOfDivisions / 2;

            View = screen.SetView(
                1e9 * controls.Position.Time + sample.TriggerTimeNs,
                1e6*controls.Position.Voltage + 1e6*controls.Units.Voltage * divisionsPrQuadrant,
                sample.TriggerTimeNs + 1e9*controls.Position.Time + 1e9 * controls.Units.Time * controls.NumberOfDivisions,
                1e6*controls.Position.Voltage - 1e6*controls.Units.Voltage * divisionsPrQuadrant, angle);

            var delta = Sub(View.TransformD(1, 0), View.TransformD(0, 0));
            Iterator = new SubSampleIterator(View, sample, delta);
        }

        (int, int) Sub((int, int) a, (int, int) b) =>
            (a.Item1 - b.Item1, a.Item2 - b.Item2);

        public void Next() =>
            Iterator.Next();

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

            private int Pixel(int sumDelta) =>
                sumDelta > 3 || sumDelta == -4 ? 0 : 1;

            // 
            //    a   b
            //  c d e f g
            //    h   i  
            //  j k l m n
            //    o   p  
            // 

            SampleIterator bI, fI, gI, iI, mI, nI, pI;
            int a, b, c, d, e, f, g, h, i, j, k, l, m, n, o, p;

            public void NewLine(int lineNo) {
                bI = new SampleIterator(Sample, View.TransformD(0.5, lineNo - 1), Delta);
                fI = new SampleIterator(Sample, View.TransformD(0.5, lineNo - 0.5), Delta);
                iI = new SampleIterator(Sample, View.TransformD(0.5, lineNo), Delta);
                mI = new SampleIterator(Sample, View.TransformD(0.5, lineNo + .5), Delta);
                pI = new SampleIterator(Sample, View.TransformD(0.5, lineNo + 1), Delta);
                gI = new SampleIterator(Sample, View.TransformD(1, lineNo - 0.5), Delta);
                nI = new SampleIterator(Sample, View.TransformD(1, lineNo + 0.5), Delta);
            }

            public void Next() {
                a = b;
                c = e; d = f; e = g;
                h = i;
                j = l; k = m; l = n;
                o = p;
                bI.Next(); b = bI.Get();
                fI.Next(); f = fI.Get(); gI.Next(); g = gI.Get();
                iI.Next(); i = iI.Get();
                mI.Next(); m = mI.Get(); nI.Next(); n = nI.Get();
                pI.Next(); p = pI.Get();
            }

            class SampleIterator {
                readonly Sampling Sampling;
                readonly (int t, int v) Delta;

                (int t, int v) Current;

                public SampleIterator(Sampling sampling, (int t, int v) start, (int t, int v) delta) {
                    Delta = delta;
                    Current = start;
                    Sampling = sampling;
                }

                public void Next() {
                    Current.t += Delta.t;
                    Current.v += Delta.v;
                }

                public int Get() =>
                    Sampling.TryGet(Current.t, out var sv) ? Math.Sign(sv - Current.v) : 8;

            }
        }
    }
}
