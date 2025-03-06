using MAUI_Tools;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls.Shapes;
using Math = Tools.Math;
using static Tools.Logger.Logger;
using static MAUI_Tools.FilesHelper;
using Tools.Logger;

namespace DiagnosticApp;

public partial class WiFiSignal : ContentPage
{
    readonly int minFrequency = 100;
    readonly int maxFrequency = 2000;
    readonly int stepFrequency = 100;

    public ObservableCollection<string> Ticks { get; } = new();

    public WiFiSignal()
    {
        bool IsValidStep = (maxFrequency - minFrequency) % stepFrequency == 0;
        if (!IsValidStep)
        {
            Log("Invalid step frequency", Severity.FATAL_ERROR);
            //Fallback
            stepFrequency = 100;
        }

        // Chiamata asincrona a AppendText
        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        InitializeComponent();

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

        _ = UpdateWifiData();
    }

    private async Task UpdateWifiData()
    {
        int frequency;
        while (true)
        {
            frequency = (int)sldUpdateFrequency.Value;
            var (signal, ip) = await WiFiHelper.GetWiFiData(); // Usa await

            await Dispatcher.DispatchAsync(() =>
            {
                lblWiFiSignal.Text = $"IP: {ip}\nSegnale: {signal}%";
            });

            await Task.Delay(frequency);
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
            Log($"Error while animating thumb\nError: {ex.Message}", Severity.CRITICAL);
        }
    }
}