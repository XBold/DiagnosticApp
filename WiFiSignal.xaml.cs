using MAUI_Tools;
using System.Collections.ObjectModel;
using Math = Tools.Math;
using static MAUI_Tools.MAUILogger;
using static Tools.Logger.Severity;

namespace DiagnosticApp;

public partial class WiFiSignal : ContentPage
{
    readonly int minFrequency = 200;
    readonly int maxFrequency = 2000;
    int stepFrequency = 200;
    private CancellationTokenSource cts = new();

    public ObservableCollection<string> Ticks { get; } = new();

    public WiFiSignal()
    {
        InitializeComponent();
    }

    private void OtherInizializations()
    {
        CheckStepFrequency();
        // Inizializza i ticks
        for (int i = minFrequency; i <= maxFrequency; i += stepFrequency)
        {
            Ticks.Add($"{i}");
        }

        sldUpdateFrequency.Minimum = minFrequency;
        sldUpdateFrequency.Maximum = maxFrequency;
        sldUpdateFrequency.Value = minFrequency;

        //// Usa await per chiamare SldUpdate
        //await SldUpdate(sldUpdateFrequency, new ValueChangedEventArgs(0, minFrequency));

        _ = UpdateWifiData(cts.Token);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        cts = new();
        OtherInizializations();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        cts.Cancel();
    }

    private void CheckStepFrequency()
    {
        bool IsValidStep = (maxFrequency - minFrequency) % stepFrequency == 0;
        if (!IsValidStep)
        {
            Log($"{stepFrequency} is not a valid step frequency - Fallback to {minFrequency}", FATAL_ERROR);
            //Fallback
            stepFrequency = minFrequency;
        }
    }

    private async Task UpdateWifiData(CancellationToken cancellationToken)
    {
        int updateFrequency;
        uint counter = 0;
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                updateFrequency = (int)sldUpdateFrequency.Value;
                WiFiHelper wiFiHelper = new();
                string text = $"IP: {wiFiHelper.GetIPAddress()}";
                text += $"\nRSSI: {wiFiHelper.GetRSSI()} dBm";
                text += $"\nRSSI percentage: {wiFiHelper.GetRSSIPerc()}%";
                text += $"\nFrequency: {wiFiHelper.GetFrequency()} MHz";
                text += $"\nBand: {wiFiHelper.GetBand()}";
                text += $"\nWiFi Standard: {wiFiHelper.GetWiFiStandard()}";
                text += $"\nSSID: {wiFiHelper.GetSSID()}";
                lblWiFiSignal.Text = text;
                lblWiFiSignal.Text += $"\nCounter: {counter++}";


                Log(text.Replace(Environment.NewLine, " - "), INFO, filePathAndName: "WiFiSignal.txt");

                // Usa il token anche nel delay per poter interrompere il task
                await Task.Delay(updateFrequency, cancellationToken);
            }
        }
        catch (TaskCanceledException)
        {
            LogConsole("UpdateWiFiData cancelled correctly", INFO);
        }
        catch (Exception ex)
        {
            Log($"Error on UpdateWifiData: {ex.Message}", CRITICAL);
        }
    }

    private async void SldUpdate(object sender, ValueChangedEventArgs e)
    {
        if (sender is Slider slider)
        {
            // Snap al valore più vicino
            var snappedValue = Math.Round(e.NewValue / stepFrequency) * stepFrequency;
            //DEBUG
            if (snappedValue != e.NewValue)
            {
                lblRawValue.Text = e.NewValue.ToString();
            }
            snappedValue = Math.Limit(minFrequency, snappedValue, maxFrequency);
            slider.Value = snappedValue;

            // Aggiorna label
            lblFrequency.Text = $"Frequenza aggiornamento: {snappedValue} ms";

            // Animazioni
            await AnimateThumb();
        }
    }

    private async Task AnimateThumb()
    {
        try
        {
            // Animazione combinata
            await Task.WhenAll(
                sldUpdateFrequency.ScaleTo(1.05, 100, Easing.SinIn),
                sldUpdateFrequency.TranslateTo(0, -5, 100, Easing.SinIn)
            );

            await Task.WhenAll(
                sldUpdateFrequency.ScaleTo(1, 200, Easing.SpringOut),
                sldUpdateFrequency.TranslateTo(0, 0, 200, Easing.SpringOut)
            );

            //await lblFrequency.RippleEffect(Color.FromArgb("#4CAF50"), 500);
        }
        catch (Exception ex)
        {
            Log($"Error while animating thumb\nError: {ex.Message}", CRITICAL);
        }
    }
}