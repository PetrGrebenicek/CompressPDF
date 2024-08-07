using System;
using System.Windows;
using System.Windows.Interop;

namespace CompressPDF
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class License : Window
    {
        public License()
        {
            InitializeComponent();
            this.Loaded += License_Loaded;
        }

        private void License_Loaded(object sender, RoutedEventArgs e)
        {
            AdjustWindowPosition();
        }

        private void AdjustWindowPosition()
        {
            var workingArea = SystemParameters.WorkArea;

            // Retrieve window dimensions
            var windowWidth = this.ActualWidth;
            var windowHeight = this.ActualHeight;

            // Check and adjust left position
            if (this.Left < workingArea.Left)
            {
                this.Left = workingArea.Left;
            }
            else if (this.Left + windowWidth > workingArea.Right)
            {
                this.Left = workingArea.Right - windowWidth;
            }

            // Check and adjust top position
            if (this.Top < workingArea.Top)
            {
                this.Top = workingArea.Top;
            }
            else if (this.Top + windowHeight > workingArea.Bottom)
            {
                this.Top = workingArea.Bottom - windowHeight;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            try
            {
                System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = e.Uri.AbsoluteUri,
                    UseShellExecute = true
                };
                System.Diagnostics.Process.Start(psi);
            }
            catch
            {
            }
            e.Handled = true;
        }
    }
}
