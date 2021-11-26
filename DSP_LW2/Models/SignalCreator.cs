using DSP_LW2.Signals;

namespace DSP_LW2.Models
{
    public static class SignalCreator
    {
        public static Signal GetSinusoid(ParametersModel model)
        {
            return new SinusoidSignal(model.A, model.F, model.P, model.N);
        }

        public static Signal GetPulse(ParametersModel model)
        {
            return new PulseWithDifferentDutyCycleSignal(model.A, model.F, model.P, model.N, model.WellRate);
        }

        public static Signal GetTriangle(ParametersModel model)
        {
            return new TriangleSignal(model.A, model.F, model.P, model.N);
        }

        public static Signal GetSawTooth(ParametersModel model)
        {
            return new SawToothSignal(model.A, model.F, model.P, model.N);
        }

        public static Signal GetNoise(ParametersModel model)
        {
            return new NoiseSignal(model.A, model.F, model.P, model.N);
        }
    }
}
