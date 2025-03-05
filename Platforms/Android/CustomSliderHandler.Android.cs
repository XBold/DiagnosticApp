using Android.Content.Res;
using Android.Widget;
using Microsoft.Maui.Handlers;
using Color = Android.Graphics.Color;

namespace DiagnosticApp.Platforms.Android;

public class CustomSliderHandler : SliderHandler
{
    protected override void ConnectHandler(SeekBar platformView)
    {
        base.ConnectHandler(platformView);

        // Personalizzazione Thumb
        platformView.Thumb?.SetTint(Color.ParseColor("#2196F3"));

        // Personalizzazione Track
        platformView.ProgressTintList = ColorStateList.ValueOf(Color.ParseColor("#BBDEFB")); // Parte riempita
        platformView.ProgressBackgroundTintList = ColorStateList.ValueOf(Color.ParseColor("#E0E0E0")); // Parte vuota
    }
}