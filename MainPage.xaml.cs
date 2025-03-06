using static MAUI_Tools.PermissionHelper;
using static MAUI_Tools.MAUILogger;
using static Tools.Logger.Severity;
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
                typeof(Permissions.NearbyWifiDevices)
            ];
            if (!await CheckPermissions(permisions))
            {
                Application.Current.Quit();
            }

            Log("Permissions granted", INFO);
            //Wait until permission is granted to initialize component
            InitializeComponent();
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
