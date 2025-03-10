using MAUI_Tools;
using System.Collections.ObjectModel;
using Math = Tools.Math;
using static MAUI_Tools.MAUILogger;
using static Tools.Logger.Severity;

namespace DiagnosticApp;

public partial class WiFiSignal : ContentPage
{
    int minFrequency = 200;
    int maxFrequency = 2000;
    int stepFrequency = 100;
    private CancellationTokenSource cts = new();
    double memoValue;

    public ObservableCollection<string> Ticks { get; } = new();

    public WiFiSignal()
    {
        InitializeComponent();
        OtherInizializations();
    }

    private void OtherInizializations()
    {
        CheckStepFrequency();
        // Inizializza i ticks
        for (int i = minFrequency; i <= maxFrequency; i += stepFrequency)
        {
            Ticks.Add($"{i}");
        }

        sldUpdateFrequency.ValueChanged -= SldUpdate;
        sldUpdateFrequency.Minimum = minFrequency;
        sldUpdateFrequency.Maximum = maxFrequency;
        sldUpdateFrequency.Value = minFrequency;
        memoValue = sldUpdateFrequency.Value;
        sldUpdateFrequency.ValueChanged += SldUpdate;

        //// Usa await per chiamare SldUpdate
        //await SldUpdate(sldUpdateFrequency, new ValueChangedEventArgs(0, minFrequency));
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        cts = new();
        _ = UpdateWifiData(cts.Token);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        cts.Cancel();
    }

    private void CheckStepFrequency()
    {
        //Check that the order of min and max is correct
        (minFrequency, maxFrequency) = Math.SortMinMax(minFrequency, maxFrequency);

        bool IsValidStep = (maxFrequency - minFrequency) % stepFrequency == 0;
        if (!IsValidStep)
        {
            maxFrequency = ((maxFrequency / stepFrequency) * stepFrequency) + minFrequency;
            Log($"{stepFrequency} is not a valid step frequency - Fallback changing max frequency to {maxFrequency}", CRITICAL);
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
                string text = $"IP: {WiFiHelper.GetIPAddress()}";
                text += $"\nRSSI: {WiFiHelper.GetRSSI(swtOldSkd.IsToggled)} dBm";
                text += $"\nRSSI percentage: {wiFiHelper.GetRSSIPerc(swtOldSkd.IsToggled)}%";
                text += $"\nFrequency: {WiFiHelper.GetFrequency(swtOldSkd.IsToggled)} MHz";
                text += $"\nBand: {WiFiHelper.GetBand(swtOldSkd.IsToggled)}";
                text += $"\nWiFi Standard: {WiFiHelper.GetWiFiStandard(swtOldSkd.IsToggled)}";
                text += $"\nSSID: {WiFiHelper.GetSSID(swtOldSkd.IsToggled)}";
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
            if (Math.Abs(e.NewValue - memoValue) < double.Epsilon)
                return;

            var snappedValue = Math.Round((e.NewValue - minFrequency) / stepFrequency) * stepFrequency + minFrequency;
            snappedValue = Math.Limit(minFrequency, snappedValue, maxFrequency);

            if (Math.Abs(slider.Value - snappedValue) > double.Epsilon)
            {
                slider.ValueChanged -= SldUpdate;
                slider.Value = snappedValue;
                slider.ValueChanged += SldUpdate;
            }

            if (snappedValue != e.NewValue)
            {
                lblRawValue.Text = slider.Value.ToString();
            }

            if (snappedValue != memoValue)
            {
                lblFrequency.Text = $"Frequenza aggiornamento: {snappedValue} ms";
                await AnimateThumb();
            }

            memoValue = snappedValue;
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