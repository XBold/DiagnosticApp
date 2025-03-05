using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using UIKit;

namespace DiagnosticApp.Platforms.iOS;

public class CustomSliderHandler : SliderHandler
{
    protected override void ConnectHandler(UISlider platformView)
    {
        base.ConnectHandler(platformView);

        // Personalizzazione iOS
        platformView.ThumbTintColor = UIColor.FromRGB(33, 150, 243);
        platformView.MinimumTrackTintColor = UIColor.FromRGB(187, 222, 251);
        platformView.MaximumTrackTintColor = UIColor.FromRGB(224, 224, 224);
    }
}