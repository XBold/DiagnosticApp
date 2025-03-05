using static MAUI_Tools.PermissionHelper;
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
                typeof(Permissions.LocationWhenInUse),
                typeof(Permissions.NetworkState),
                typeof(Permissions.StorageWrite)
            ];
            if (!await CheckPermissions(permisions))
            {
                Application.Current.Quit();
            }
            //Wait until permission is granted to initialize componenst
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
