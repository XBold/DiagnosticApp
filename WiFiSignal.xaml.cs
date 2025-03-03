using MAUI_Tools;
using Math = Tools.Math;
namespace DiagnosticApp;

public partial class WiFiSignal : ContentPage
{
    int minFrequency = 100;
    int maxFrequency = 2000;
	public WiFiSignal()
	{
		InitializeComponent();
        sldUpdateFrequency.Minimum = minFrequency;
        sldUpdateFrequency.Maximum = maxFrequency;
        sldUpdateFrequency.Value = minFrequency;
        SldUpdate(sldUpdateFrequency, new ValueChangedEventArgs(0, minFrequency));  //Initial startup
        _ = UpdateWifiData();
    }

    private async Task UpdateWifiData()
    {
        int frequency;
        while (true)
        {
            frequency = Math.Limit(min: minFrequency, value: (int)sldUpdateFrequency.Value, max: maxFrequency);
            var (signal, ip) = WiFiHelper.GetWiFiData();
            lblWiFiSignal.Text = $"IP: {ip}\nSignal: {signal.ToString()}";
            await Task.Delay(frequency);
        }
    }

    private void SldUpdate(object sender, ValueChangedEventArgs e)
    {
        int frequency = (int)e.NewValue;
        lblFrequency.Text = $"Frequency: {frequency} ms";
    }
}