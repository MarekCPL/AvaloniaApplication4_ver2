using Avalonia.Controls;
using Avalonia.Interactivity;
using ScottPlot;
using System;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace AvaloniaApplication4.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            InitializePlot();

            AmplitudeSlider.ValueChanged += UpdatePlot;
            FrequencySlider.ValueChanged += UpdatePlot;
            WaveformSelector.SelectionChanged += UpdatePlot;
        }

        private void InitializePlot()
        {
            PlotControl.Plot.Title("Generator Sygna³u");
            PlotControl.Plot.XLabel("Czas [s]");
            PlotControl.Plot.YLabel("Amplituda");
            UpdatePlot(null, null);
        }

        private void UpdatePlot(object? sender, EventArgs? e)
        {
            double amplitude = AmplitudeSlider.Value;
            double frequency = FrequencySlider.Value;
            string waveformType = (WaveformSelector.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Sinusoidalna";

            // Lepsza rozdzielczoœæ wykresu
            int samplesPerCycle = 200;
            int numCycles = 5;
            int sampleCount = (int)(samplesPerCycle * numCycles);

            double[] xs = new double[sampleCount];
            double[] ys = new double[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                xs[i] = i / (frequency * samplesPerCycle);
                ys[i] = waveformType switch
                {
                    "Sinusoidalna" => amplitude * Math.Sin(2 * Math.PI * frequency * xs[i]),
                    "Trójk¹tna" => amplitude * (2 * Math.Asin(Math.Sin(2 * Math.PI * frequency * xs[i]))) / Math.PI,
                    "Prostok¹tna" => amplitude * Math.Sign(Math.Sin(2 * Math.PI * frequency * xs[i])),
                    _ => 0
                };
            }

            PlotControl.Plot.Clear();
            var scatter = PlotControl.Plot.Add.Scatter(xs, ys);
            scatter.LineWidth = 2;
            scatter.Color = ScottPlot.Colors.Blue;

            PlotControl.Plot.Axes.AutoScale();
            PlotControl.Refresh();
        }

        private void OnPlaySoundClick(object? sender, RoutedEventArgs e)
        {
            double amplitude = AmplitudeSlider.Value;
            double frequency = FrequencySlider.Value;
            string waveformType = (WaveformSelector.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Sinusoidalna";

            PlaySignal(amplitude, frequency, waveformType);
        }

        public void PlaySignal(double amplitude, double frequency, string waveformType, int durationSeconds = 2)
        {
            int sampleRate = 44100;

            var signal = new SignalGenerator(sampleRate, 1)
            {
                Gain = amplitude,
                Frequency = frequency,
                Type = waveformType switch
                {
                    "Sinusoidalna" => SignalGeneratorType.Sin,
                    "Trójk¹tna" => SignalGeneratorType.Triangle,
                    "Prostok¹tna" => SignalGeneratorType.Square,
                    _ => SignalGeneratorType.Sin
                }
            };

            int totalSamples = sampleRate * durationSeconds;

            var trimmed = new OffsetSampleProvider(signal)
            {
                TakeSamples = totalSamples
            };

            var waveOut = new WaveOutEvent();
            waveOut.Init(trimmed);
            waveOut.Play();
        }
    }
}
