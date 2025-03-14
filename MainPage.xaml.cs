using static MAUI_Tools.PermissionHelper;
using static MAUI_Tools.MAUILogger;
using static Tools.Logger.Severity;
using Plugin.LocalNotification;

namespace DiagnosticApp
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            _ = CheckPermissionsStartup();
        }

        private async Task CheckPermissionsStartup()
        {
            Type[] permisions =
            [
                typeof(Permissions.NetworkState),
                typeof(Permissions.NearbyWifiDevices),
                typeof(Permissions.PostNotifications)
            ];
            if (!await CheckPermissions(permisions))
            {
                Application.Current.Quit();
            }

            //Wait until permission is granted to initialize component
            InitializeComponent();
            Log("App started", INFO);
        }
        private void OnCounterClicked(object sender, EventArgs e)
        {
            count++;

            if (count == 1)
                CounterBtn.Text = $"Clicked {count} time";
            else
                CounterBtn.Text = $"Clicked {count} times";

            SemanticScreenReader.Announce(CounterBtn.Text);
        }
    }
}
