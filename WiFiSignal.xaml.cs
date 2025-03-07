using MAUI_Tools;
using System.Collections.ObjectModel;
using Math = Tools.Math;
using static MAUI_Tools.MAUILogger;
using static Tools.Logger.Severity;

namespace DiagnosticApp;

public partial class WiFiSignal : ContentPage
{
    readonly int minFrequency = 500;
    readonly int maxFrequency = 2000;
    int stepFrequency = 500;
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
        Log("Intercettato l'ingresso nella pagina", INFO);
        cts = new();
        OtherInizializations();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        // Quando la pagina scompare, annulla i task in corso
        cts.Cancel();
        Log("Intercettato il leave della pagina: processi annullati", INFO);
    }

    private void CheckStepFrequency()
    {
        bool IsValidStep = (maxFrequency - minFrequency) % stepFrequency == 0;
        if (!IsValidStep)
        {
            Log($"{stepFrequency} is not a valid frequency - Fallback to {minFrequency}", FATAL_ERROR);
            //Fallback
            stepFrequency = minFrequency;
        }
    }

    private async Task UpdateWifiData(CancellationToken cancellationToken)
    {
        int frequency;
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                frequency = (int)sldUpdateFrequency.Value;
                WiFiHelper wiFiHelper = new();
                string signal = wiFiHelper.GetRSSI().ToString();
                string ip = wiFiHelper.GetIPAddress();
                lblWiFiSignal.Text = $"IP: {ip}\nSegnale: {signal}%";
                Log($"RSSI: {signal}", INFO);

                // Usa il token anche nel delay per poter interrompere il task
                await Task.Delay(frequency, cancellationToken);
            }
        }
        catch (TaskCanceledException)
        {
            Log("UpdateWifiData annullato", INFO);
        }
        catch (Exception ex)
        {
            Log($"Errore in UpdateWifiData: {ex.Message}", CRITICAL);
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