using System.IO;
using System.Windows;

namespace CompressPDF
{
    public static class ErrorHandler
    {
        #region Log and Show Error
        public static void LogAndShowError(Exception ex)
        {
            // Construct the log file name with the current timestamp
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
            string logFileName = $"error-log-{timestamp}.txt";

            // Get the application's base directory
            string appDirectory = AppDomain.CurrentDomain.BaseDirectory;

            // Combine the directory and file name to get the full path
            string logFilePath = Path.Combine(appDirectory, logFileName);

            // Get the application name
            string applicationName = AppDomain.CurrentDomain.FriendlyName;

            // Get the application version (assuming you have it defined in your assembly info)
            string appVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Unknown Version";

            // Get the OS and .NET version
            string osVersion = Environment.OSVersion.ToString();
            string dotNetVersion = Environment.Version.ToString();

            // Create a detailed error message
            string errorMessage = $"Application Name: {applicationName}\n" +
                                  $"Application Version: {appVersion}\n" +
                                  $"Operating System: {osVersion}\n" +
                                  $".NET Version: {dotNetVersion}\n" +
                                  $"Date and Time: {DateTime.Now}\n" +

                                  $"Exception Type: {ex.GetType()}\n\n\n" +
                                  $"Exception Message: {ex.Message}\n\n\n" +
                                  $"Inner Exception: {ex.InnerException?.Message}\n\n\n" +
                                  $"Stack Trace: {ex.StackTrace}\n\n\n" +
                                  $"Target Site: {ex.TargetSite}\n\n\n" +
                                  $"Source: {ex.Source}";

            // Log the detailed exception to the file
            File.WriteAllText(logFilePath, errorMessage);

            // Show the message box with the specified details
            string message = $"An unhandled error has occurred, and the error report has been saved to: {logFilePath}";
            MessageBox.Show(message, "An error occurred", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        #endregion
    }
}