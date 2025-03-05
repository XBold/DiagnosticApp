using MAUI_Tools;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls.Shapes;
using Math = Tools.Math;
using static Tools.Logger.Logger;
using Tools.Logger;
using static MAUI_Tools.FilesHelper;

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
        AppendText("log.txt", "PIPPO");
        InitializeComponent();

        // Inizializza i ticks
        for (int i = minFrequency; i <= maxFrequency; i += stepFrequency)
        {
            Ticks.Add($"{i}");
        }

        sldUpdateFrequency.Minimum = minFrequency;
        sldUpdateFrequency.Maximum = maxFrequency;
        sldUpdateFrequency.Value = minFrequency;
        SldUpdate(sldUpdateFrequency, new ValueChangedEventArgs(0, minFrequency));
        _ = UpdateWifiData();
    }

    private async Task UpdateWifiData()
    {
        int frequency;
        while (true)
        {
            frequency = (int)sldUpdateFrequency.Value;
            var (signal, ip) = WiFiHelper.GetWiFiData();

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

            await lblFrequency.RippleEffect(Color.FromArgb("#4CAF50"), 500);
        }
        catch (Exception ex)
        {
            Log($"Error while animating thumb\nError: {ex.Message}", Severity.CRITICAL);
        }
    }
}

// Estensione per l'effetto ripple
public static class AnimationExtensions
{
    public static async Task RippleEffect(this View element, Color rippleColor, uint duration)
    {
        try
        {
            if (element?.Parent is not Microsoft.Maui.Controls.Layout parentLayout) return;

            var ripple = new Ellipse
            {
                BackgroundColor = rippleColor,
                Opacity = 0.5,
                Scale = 0,
                AnchorX = 0.5,
                AnchorY = 0.5,
                WidthRequest = 50,
                HeightRequest = 50
            };

            parentLayout.Children.Add(ripple);

            var position = element.GetAbsolutePosition();
            ripple.TranslationX = position.X + (element.Width / 2) - 25;
            ripple.TranslationY = position.Y + (element.Height / 2) - 25;

            await Task.WhenAll(
                ripple.ScaleTo(3, duration, Easing.CubicOut),
                ripple.FadeTo(0, duration)
            );
        }
        finally
        {
            if (element?.Parent is Microsoft.Maui.Controls.Layout parentLayout && parentLayout.Children.LastOrDefault() is Ellipse ripple)
            {
                parentLayout.Children.Remove(ripple);
            }
        }
    }

    public static Point GetAbsolutePosition(this View element)
    {
        var position = element.Bounds.Location;
        var parent = element.Parent as View;

        while (parent != null)
        {
            position.X += parent.Bounds.X;
            position.Y += parent.Bounds.Y;
            parent = parent.Parent as View;
        }

        return position;
    }
}