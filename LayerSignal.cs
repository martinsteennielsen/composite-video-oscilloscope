using System;

namespace CompositeVideoOscilloscope {

    public class LayerSignal {
        private readonly Viewport View;
        private readonly Sampling Sample;
        private readonly (int, int) Delta;

        public LayerSignal(Viewport screen, Sampling sample, PlotControls controls, double angle, VideoStandard standard) {
            Sample = sample;
            var divisionsPrQuadrant = controls.NumberOfDivisions / 2;

            var triggerTimeNs = sample.RunTrigger(controls.Trigger);

            View = screen.SetView(
                1e9 * controls.Position.Time + triggerTimeNs,
                1e6 * controls.Position.Voltage + 1e6 * controls.Units.Voltage * divisionsPrQuadrant,
                triggerTimeNs + 1e9 * controls.Position.Time + 1e9 * controls.Units.Time * controls.NumberOfDivisions,
                1e6 * controls.Position.Voltage - 1e6 * controls.Units.Voltage * divisionsPrQuadrant, angle);

            Delta = Subtract(View.TransformD(1, 0), View.TransformD(0, 0));
        }

        (int, int) Subtract((int, int) a, (int, int) b) =>
            (a.Item1 - b.Item1, a.Item2 - b.Item2);

        public void Next(SignalPlotIterator iter) {
            iter.a = iter.b;
            iter.c = iter.e; iter.d = iter.f; iter.e = iter.g;
            iter.h = iter.i;
            iter.j = iter.l; iter.k = iter.m; iter.l = iter.n;
            iter.o = iter.p;
            iter.b = Sample.GetNext(iter.iterB);
            iter.f = Sample.GetNext(iter.iterF);
            iter.g = Sample.GetNext(iter.iterG);
            iter.i = Sample.GetNext(iter.iterI);
            iter.m = Sample.GetNext(iter.iterM);
            iter.n = Sample.GetNext(iter.iterN);
            iter.p = Sample.GetNext(iter.iterP);
        }

        public void Reset(SignalPlotIterator iter, int startX, int startY) {
            Sample.Reset(iter.iterB, View.TransformD(startX + 0.5, startY - 1), Delta);
            Sample.Reset(iter.iterF, View.TransformD(startX + 0.5, startY - 0.5), Delta);
            Sample.Reset(iter.iterI, View.TransformD(startX + 0.5, startY), delta: Delta);
            Sample.Reset(iter.iterM, View.TransformD(startX + 0.5, startY + 0.5), Delta);
            Sample.Reset(iter.iterP, View.TransformD(startX + 0.5, startY + 1), Delta);
            Sample.Reset(iter.iterG, View.TransformD(startX + 1, startY - 0.5), Delta);
            Sample.Reset(iter.iterN, View.TransformD(startX + 1, startY + 0.5), Delta);
            iter.a = iter.b = iter.c = iter.d = iter.e = iter.f = iter.g = iter.h = iter.i = iter.j = iter.k = iter.l = iter.n = iter.o = iter.p = 8;
        }

        public int Get(SignalPlotIterator iter) =>
            Get_(iter) << 5;

        public int Get_(SignalPlotIterator iter) =>
            Pixel(iter.a + iter.c + iter.h + iter.e) +
            Pixel(iter.e + iter.b + iter.i + iter.g) +
            Pixel(iter.j + iter.h + iter.l + iter.o) +
            Pixel(iter.l + iter.n + iter.i + iter.p) +
            (Pixel(iter.d + iter.f + iter.k + iter.m) << 2);

        private byte Pixel(int sumDelta) =>
            sumDelta > 3 || sumDelta == -4 ? (byte)0 : (byte)1;

    }
}
