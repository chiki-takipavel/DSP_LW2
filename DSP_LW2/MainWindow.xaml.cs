using DSP_LW2.Models;
using DSP_LW2.Signals;
using DSP_LW2.Streams;
using DSP_LW2.ViewModels;
using NAudio.Wave;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Media;
using System.Windows;

namespace DSP_LW2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private delegate Signal GetSignal(ParametersModel model);

        private readonly List<GetSignal> getSignals = new()
        {
            SignalCreator.GetSinusoid,
            SignalCreator.GetPulse,
            SignalCreator.GetTriangle,
            SignalCreator.GetSawTooth,
            SignalCreator.GetNoise
        };

        public ObservableCollection<ParametersModel> Signals { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            Signals = new ObservableCollection<ParametersModel>()
            {
                new ParametersModel()
            };

            listSignals.ItemsSource = Signals;
        }

        private void AddClick(object sender, RoutedEventArgs e)
        {
            Signals.Add(new ParametersModel());
        }

        private void DeleteClick(object sender, RoutedEventArgs e)
        {
            if (Signals.Count > 1)
            {
                Signals.RemoveAt(Signals.Count - 1);
            }
        }

        private void DrawClick(object sender, RoutedEventArgs e)
        {
            int n = int.Parse(tbN.Text);
            PolyHarmonicSignal signal = GetPolyHarmonicSignal(Signals, n);
            signal.GenerateSpectrum((bool)chbFft.IsChecked);

            List<float> signalValues = signal.Values.Select(value => (float)value).ToList();
            List<float> amplitudesValues = signal.AmplitudeSpectrum.Select(value => value < 0.0001 ? 0 : (float)value).ToList();
            List<float> phasesValues = signal.PhaseSpectrum.Select(value => (float)value).ToList();
            List<float> restoredValues = signal.RestoreSignal.Select(value => (float)value).ToList();
            List<float> restoredNonPhasedValues = signal.RestoreNonPhasedSignal.Select(value => (float)value).ToList();

            ChartViewModel chart = (ChartViewModel)DataContext;
            chart.CreateSignalsChart(signalValues, restoredValues, restoredNonPhasedValues);
            chart.CreateSpectrumChart(amplitudesValues, phasesValues);
        }

        private void PlayClick(object sender, RoutedEventArgs e)
        {
            PolyHarmonicSignal signal = GetPolyHarmonicSignal(Signals, 32768);
            signal.GenerateSpectrum(true);
            List<float> values = signal.RestoreSignal.Select(value => (float)value).ToList();
            PlaySound(values);
        }

        private static void PlaySound(List<float> samples)
        {
            ushort numChannels = 1;
            int sampleRate = 32768;

            WaveFormat waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, numChannels);
            ISampleProvider stream = new MyWaveStream(waveFormat, samples);
            WaveFileWriter.CreateWaveFile16("Result.wav", stream);

            SoundPlayer player = new("Result.wav");
            player.Play();
        }

        private PolyHarmonicSignal GetPolyHarmonicSignal(IEnumerable<ParametersModel> models, int n)
        {
            List<Signal> signals = new();
            foreach (var model in models)
            {
                model.N = n;
                signals.Add(getSignals[model.SignalType](model));
            }

            FiltrationType filtrationType = (FiltrationType)cmbSignalType.SelectedIndex;
            _ = int.TryParse(tbMinHarmonic.Text, out int minHarmonic);
            _ = int.TryParse(tbMaxHarmonic.Text, out int maxHarmonic);
            PolyHarmonicSignal polyHarmonicSignal = new(signals, filtrationType, minHarmonic, maxHarmonic);
            return polyHarmonicSignal;
        }
    }
}
