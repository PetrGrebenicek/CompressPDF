using System.Diagnostics;
using System.Windows;

namespace CompressPDF
{
    public partial class UpdateWindow : Window
    {
        public UpdateWindow(Window owner, Version currentVersion, Version latestVersion)
        {
            InitializeComponent();
            Owner = owner;
            WindowStyle = WindowStyle.SingleBorderWindow;
            ResizeMode = ResizeMode.NoResize;

            // This prevents interaction with the owner window
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            WindowState = WindowState.Normal;

            CurrentVersionTextBlock.Text = $"Current Version: {currentVersion}";
            LatestVersionTextBlock.Text = $"Latest Version: {latestVersion}";
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = Update.DownloadLink,
                UseShellExecute = true
            });
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            // Close all windows and terminate the application
            foreach (Window window in Application.Current.Windows)
            {
                window.Close();
            }
            Application.Current.Shutdown();
        }
    }
}