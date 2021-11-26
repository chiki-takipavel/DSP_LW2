using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace DSP_LW2.Signals
{
    public class PolyHarmonicSignal
    {
        private const double Threshold = 0.0001;
        public int Seconds { get; set; }
        public int N { get; set; }
        public double[] Values { get; private set; }
        public double[] SinSpectrum { get; private set; }
        public double[] CosSpectrum { get; private set; }
        public double[] AmplitudeSpectrum { get; private set; }
        public double[] PhaseSpectrum { get; private set; }
        public double[] RestoreSignal { get; private set; }
        public double[] RestoreNonPhasedSignal { get; private set; }
        public FiltrationType FiltrationType { get; set; }
        public int MinHarmonic { get; set; }
        public int MaxHarmonic { get; set; }
        public int NumberHarmonics { get; set; }

        public PolyHarmonicSignal(IEnumerable<Signal> signals, FiltrationType filtrationType, int minHarmonic, int maxHarmonic)
        {
            FiltrationType = filtrationType;

            List<double[]> valueSignals = new();
            foreach(Signal signal in signals)
            {
                signal.Generate();
                valueSignals.Add(signal.Values);
            }

            N = signals.First().N;
            Seconds = signals.First().Seconds;
            NumberHarmonics = N / 2;

            MinHarmonic = Seconds * minHarmonic;
            MaxHarmonic = Seconds * maxHarmonic;

            Values = new double[N * Seconds];
            for (int i = 0; i < N * Seconds; i++)
            {
                Values[i] = 0;
                for (int j = 0; j < valueSignals.Count; j++)
                {
                    Values[i] += valueSignals[j][i];
                }
            }
        }

        public void GenerateSpectrum(bool fft)
        {
            if (fft)
            {
                Complex[] complexValues = Values.Select(value => (Complex)value).ToArray();
                Complex[] results = Fft.GetFft(complexValues);
                AmplitudeSpectrum = GetAmplitudeFftSpectrum(results);
                PhaseSpectrum = GetPhaseFftSpectrum(results);
                RestoreSignal = GetRestoredFftSignal(results, true);
                RestoreNonPhasedSignal = GetRestoredFftSignal(results, false);
            }
            else
            {
                SinSpectrum = GetSinSpectrum();
                CosSpectrum = GetCosSpectrum();
                AmplitudeSpectrum = GetAmplitudeSpectrum();
                PhaseSpectrum = GetPhaseSpectrum();
                RestoreSignal = GetRestoredSignal(true);
                RestoreNonPhasedSignal = GetRestoredSignal(false);
            }
        }

        public double[] GetSinSpectrum()
        {
            double[] values = new double[NumberHarmonics];
            for (int j = 0; j < NumberHarmonics; j++)
            {
                double value = 0;
                for (int i = 0; i < N; i++)
                {
                    value += Values[i] * Math.Sin(2 * Math.PI * i * j / N);
                }

                values[j] = 2 * value / N;
            }

            return values;
        }

        public double[] GetCosSpectrum()
        {
            double[] values = new double[NumberHarmonics];
            for (int j = 0; j < NumberHarmonics; j++)
            {
                double value = 0;
                for (int i = 0; i < N; i++)
                {
                    value += Values[i] * Math.Cos(2 * Math.PI * i * j / N);
                }

                values[j] = 2 * value / N;
            }

            return values;
        }

        public double[] GetAmplitudeFftSpectrum(Complex[] values)
        {
            double[] resultValues = new double[values.Length];
            double tempValue;
            for (int j = 0; j < values.Length; j++)
            {
                int index;
                if (j > values.Length / 2)
                {
                    index = values.Length - j;
                }
                else
                {
                    index = j;
                }

                tempValue = values[j].Magnitude * 2 / values.Length;
                switch (FiltrationType)
                {
                    case FiltrationType.BandPass:
                        resultValues[j] = (index > MaxHarmonic || index < MinHarmonic) ? 0 : tempValue;
                        break;
                    case FiltrationType.HighFrequencies:
                        resultValues[j] = index < MaxHarmonic ? 0 : tempValue;
                        break;
                    case FiltrationType.LowFrequencies:
                        resultValues[j] = index > MinHarmonic ? 0 : tempValue;
                        break;
                    case FiltrationType.None:
                        resultValues[j] = tempValue;
                        break;
                }
            }

            return resultValues;
        }

        public double[] GetPhaseFftSpectrum(Complex[] values)
        {
            var resultValues = values.Select((value, i) => Math.Atan2(value.Imaginary, value.Real)).ToArray();
            resultValues = resultValues.Select((value, i) => Math.Abs(AmplitudeSpectrum[i]) < Threshold ? 0 : value).ToArray();
            return resultValues;
        }

        public double[] GetRestoredFftSignal(Complex[] values, bool withPhase)
        {
            int k = withPhase ? 1 : 0;
            var complexResultValues = values.Select((value, i) => AmplitudeSpectrum[i] / 2 * Complex.Exp(Complex.ImaginaryOne * PhaseSpectrum[i] * k)).ToArray();
            complexResultValues = Fft.GetFftIterative(complexResultValues, true).ToArray();
            var resultValues = complexResultValues.Select(value => value.Real * complexResultValues.Length).ToArray();
            return resultValues;
        }

        public double[] GetAmplitudeSpectrum()
        {
            double[] values = new double[NumberHarmonics];
            double tempValue;
            for (int j = 0; j < NumberHarmonics; j++)
            {
                tempValue = Math.Sqrt(Math.Pow(SinSpectrum[j], 2) + Math.Pow(CosSpectrum[j], 2));
                switch (FiltrationType)
                {
                    case FiltrationType.BandPass:
                        values[j] = (j > MaxHarmonic || j < MinHarmonic) ? 0 : tempValue;
                        break;
                    case FiltrationType.HighFrequencies:
                        values[j] = j < MaxHarmonic ? 0 : tempValue;
                        break;
                    case FiltrationType.LowFrequencies:
                        values[j] = j > MinHarmonic ? 0 : tempValue;
                        break;
                    case FiltrationType.None:
                        values[j] = tempValue;
                        break;
                }
            }

            return values;
        }

        public double[] GetPhaseSpectrum()
        {
            double[] values = new double[NumberHarmonics];
            for (int j = 0; j < NumberHarmonics; j++)
            {
                values[j] = Math.Atan2(SinSpectrum[j], CosSpectrum[j]);
                if (Math.Abs(AmplitudeSpectrum[j]) < Threshold)
                {
                    AmplitudeSpectrum[j] = 0;
                    values[j] = 0;
                }
            }

            return values;
        }

        public double[] GetRestoredSignal(bool withPhase)
        {
            int k = withPhase ? 1 : 0;
            double[] values = new double[N];
            for (int i = 0; i < N; i++)
            {
                double val = 0;
                for (int j = 1; j < NumberHarmonics; j++)
                {
                    val += AmplitudeSpectrum[j] * Math.Cos(2 * Math.PI * i * j / N - PhaseSpectrum[j] * k);
                }

                val += AmplitudeSpectrum[0] / 2;
                values[i] = val;
            }

            return values;
        }
    }
}