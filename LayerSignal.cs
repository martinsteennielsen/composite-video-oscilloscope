using System;

namespace CompositeVideoOscilloscope {

    public class LayerSignal {
        private readonly Viewport View;
        private readonly Sampling Sample;
        private readonly (int, int) Delta;
        public readonly Func<SignalPlotState, int> GetNext;
        public readonly Action<SignalPlotState,int,int> ResetState;

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

            GetNext = controls.SubSamplePlot
                ? state => _GetNext(state.SubSamplingState)
                : (Func<SignalPlotState, int>)(state => _GetNext(state.SamplingState));

            ResetState = controls.SubSamplePlot
                ? (state, x, y) => _ResetState(state.SubSamplingState, x, y)
                : (Action<SignalPlotState, int, int>)((state, x, y) => _ResetState(state.SamplingState, x, y));
        }

        (int, int) Subtract((int, int) a, (int, int) b) =>
            (a.Item1 - b.Item1, a.Item2 - b.Item2);

        void _ResetState(PlotSamplingState current, int startX, int startY) {
            Sample.ResetState(current.A, View.TransformD(startX - 0.5, startY), Delta);
            Sample.ResetState(current.B, View.TransformD(startX + 0.5, startY), Delta);
            Sample.ResetState(current.C, View.TransformD(startX, startY - 0.5), delta: Delta);
            Sample.ResetState(current.D, View.TransformD(startX, startY + 0.5), Delta);
            current.a = current.b = current.c = current.d = 8;
        }

        void _ResetState(PlotSubSamplingState current, int startX, int startY) {
            Sample.ResetState(current.B, View.TransformD(startX + 0.5, startY - 1), Delta);
            Sample.ResetState(current.F, View.TransformD(startX + 0.5, startY - 0.5), Delta);
            Sample.ResetState(current.I, View.TransformD(startX + 0.5, startY), delta: Delta);
            Sample.ResetState(current.M, View.TransformD(startX + 0.5, startY + 0.5), Delta);
            Sample.ResetState(current.P, View.TransformD(startX + 0.5, startY + 1), Delta);
            Sample.ResetState(current.G, View.TransformD(startX + 1, startY - 0.5), Delta);
            Sample.ResetState(current.N, View.TransformD(startX + 1, startY + 0.5), Delta);
            current.a = current.b = current.c = current.d = current.e = current.f = current.g = current.h = current.i = current.j = current.k = current.l = current.n = current.o = current.p = 8;
        }

        int _GetNext(PlotSamplingState current) {
            var currentValue = Pixel(current.a + current.b + current.c + current.d) << 8;
            current.a = current.b;
            current.b = Sample.GetNext(current.B);
            current.c = Sample.GetNext(current.C);
            current.d = Sample.GetNext(current.D);
            return currentValue;
        }

        int _GetNext(PlotSubSamplingState current) {
            var currentValue = Get(current) << 5;
            current.a = current.b;
            current.c = current.e; current.d = current.f; current.e = current.g;
            current.h = current.i;
            current.j = current.l; current.k = current.m; current.l = current.n;
            current.o = current.p;
            current.b = Sample.GetNext(current.B);
            current.f = Sample.GetNext(current.F);
            current.g = Sample.GetNext(current.G);
            current.i = Sample.GetNext(current.I);
            current.m = Sample.GetNext(current.M);
            current.n = Sample.GetNext(current.N);
            current.p = Sample.GetNext(current.P);
            return currentValue;
        }

        int Get(PlotSubSamplingState current) =>
            Pixel(current.a + current.c + current.h + current.e) +
            Pixel(current.e + current.b + current.i + current.g) +
            Pixel(current.j + current.h + current.l + current.o) +
            Pixel(current.l + current.n + current.i + current.p) +
            (Pixel(current.d + current.f + current.k + current.m) << 2);

        private byte Pixel(int sumDelta) =>
            sumDelta > 3 || sumDelta == -4 ? (byte)0 : (byte)1;

    }
}
