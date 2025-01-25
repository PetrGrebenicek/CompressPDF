using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Net.Http;

namespace CompressPDF
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region Initialize variables
        public ObservableCollection<PDFFileInfo> FileInfoListPDF { get; set; }
        private bool isGrayscaleChecked;
        private bool isPreserveFontsChecked;
        private bool isProcessingFiles;
        public bool isUpdateAvailable = false;
        #endregion

        #region Constructor
        public MainWindow()
        {
            try
            {
                DataContext = this;
                FileInfoListPDF = [];
                if (ListViewPDF != null)
                {
                    ListViewPDF.ItemsSource = FileInfoListPDF;
                }
                InitializeComponent();
                CustomWindowSnapToTop();
                Task.Run(async () => await CheckForUpdatesAsync());
            }
            catch (Exception ex)
            {
                ErrorHandler.LogAndShowError(ex);
                throw;
            }
        }
        #endregion

        #region Resize Window
        public bool resizeWindowMouseMoveRight;
        public bool resizeWindowMouseMoveBottom;
        public bool resizeWindowMouseMoveTop;
        public bool resizeWindowMouseMoveLeft;
        public System.Windows.Point resizeWindowMouseClick;
        public bool isDraggingWindow = false;

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (isDraggingWindow == true)
            {
                return;
            }
            else if (WindowState == WindowState.Maximized)
            {
                return;
            }
            else
            {
                var mousePos = Mouse.GetPosition(this);
                resizeWindowMouseClick = mousePos;

                // Bottom-Right Corner
                if (mousePos.X >= this.Width - 4 && mousePos.Y >= this.Height - 4)
                {
                    resizeWindowMouseMoveRight = true;
                    resizeWindowMouseMoveBottom = true;
                    this.Cursor = Cursors.SizeNWSE;
                }
                // Bottom-Left Corner
                else if (mousePos.X <= 3 && mousePos.Y >= this.Height - 4)
                {
                    resizeWindowMouseMoveLeft = true;
                    resizeWindowMouseMoveBottom = true;
                    this.Cursor = Cursors.SizeNESW;
                }
                // Top-Right Corner
                else if (mousePos.X >= this.Width - 4 && mousePos.Y <= 3)
                {
                    resizeWindowMouseMoveRight = true;
                    resizeWindowMouseMoveTop = true;
                    this.Cursor = Cursors.SizeNESW;
                }
                // Top-Left Corner
                else if (mousePos.X <= 3 && mousePos.Y <= 3)
                {
                    resizeWindowMouseMoveLeft = true;
                    resizeWindowMouseMoveTop = true;
                    this.Cursor = Cursors.SizeNWSE;
                }
                // Right Edge
                else if (mousePos.X >= this.Width - 4)
                {
                    resizeWindowMouseMoveRight = true;
                    this.Cursor = Cursors.SizeWE;
                }
                // Left Edge
                else if (mousePos.X <= 3)
                {
                    resizeWindowMouseMoveLeft = true;
                    this.Cursor = Cursors.SizeWE;
                }
                // Bottom Edge
                else if (mousePos.Y >= this.Height - 4)
                {
                    resizeWindowMouseMoveBottom = true;
                    this.Cursor = Cursors.SizeNS;
                }
                // Top Edge
                else if (mousePos.Y <= 3)
                {
                    resizeWindowMouseMoveTop = true;
                    this.Cursor = Cursors.SizeNS;
                }

                // Capture the mouse to receive events even when the cursor goes outside the window
                if (resizeWindowMouseMoveRight || resizeWindowMouseMoveBottom ||
                    resizeWindowMouseMoveTop || resizeWindowMouseMoveLeft)
                {
                    CaptureMouse();
                }
            }
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (resizeWindowMouseMoveRight || resizeWindowMouseMoveBottom ||
                resizeWindowMouseMoveTop || resizeWindowMouseMoveLeft)
            {
                var mousePos = Mouse.GetPosition(this);
                var screenMousePos = PointToScreen(mousePos);

                if (resizeWindowMouseMoveRight)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        this.Width = Math.Max(MinWidth, screenMousePos.X - this.Left);
                    });
                }
                if (resizeWindowMouseMoveBottom)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        this.Height = Math.Max(MinHeight, screenMousePos.Y - this.Top);
                    });
                }
                if (resizeWindowMouseMoveTop)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        var newHeight = Math.Max(MinHeight, this.Height - (mousePos.Y - resizeWindowMouseClick.Y));
                        this.Height = newHeight;
                        this.Top = Math.Min(this.Top + (mousePos.Y - resizeWindowMouseClick.Y), this.Top + this.Height - MinHeight);
                    });
                }
                if (resizeWindowMouseMoveLeft)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        var newWidth = Math.Max(MinWidth, this.Width - (mousePos.X - resizeWindowMouseClick.X));
                        this.Width = newWidth;
                        this.Left = Math.Min(this.Left + (mousePos.X - resizeWindowMouseClick.X), this.Left + this.Width - MinWidth);
                    });
                }
            }
        }

        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            resizeWindowMouseMoveRight = false;
            resizeWindowMouseMoveBottom = false;
            resizeWindowMouseMoveTop = false;
            resizeWindowMouseMoveLeft = false;
            ReleaseMouseCapture();
            this.Cursor = Cursors.Arrow;
        }
        #endregion

        #region Custom window functions
        private System.Windows.Point startPoint;

        private void GridTopPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                return;
            }
            else
            {
                isDraggingWindow = true;
                startPoint = e.GetPosition(this);
                Mouse.Capture(GridTopPanel);
                this.Cursor = Cursors.SizeAll;
            }
        }

        static Point MousePosition()
        {
            Point mousePosition = Mouse.GetPosition(Application.Current.MainWindow);
            Point windowPosition = Application.Current.MainWindow.PointToScreen(new Point(0, 0));
            Point relativePosition = new(mousePosition.X + windowPosition.X, mousePosition.Y + windowPosition.Y);

            return relativePosition;
        }

        private void GridTopPanel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isDraggingWindow = false;
            Mouse.Capture(null);
            this.Cursor = Cursors.Arrow;

            var workAreaScreen = System.Windows.SystemParameters.WorkArea;
            double wholeScreenHeight = SystemParameters.PrimaryScreenHeight;
            var taskBarHeight = wholeScreenHeight - workAreaScreen.Height;


            // Left Top Corner
            if (MousePosition().X <= 50 && MousePosition().Y <= 50)
                CustomWindowSnapToLeftTop();

            // Left Bottom Corner
            else if (MousePosition().X <= 50 && MousePosition().Y > workAreaScreen.Height - taskBarHeight - 50)
                CustomWindowSnapToLeftBottom();

            // Left Side
            else if (MousePosition().X <= 50)
                CustomWindowSnapToLeft();

            // Right Top Border
            if (MousePosition().X >= SystemParameters.VirtualScreenWidth - 50 && MousePosition().Y <= 50)
                CustomWindowSnapToRightTop();

            // Right Bottom Corner
            else if (MousePosition().X >= SystemParameters.VirtualScreenWidth - 50 && MousePosition().Y > workAreaScreen.Height - taskBarHeight - 50)
                CustomWindowSnapToRightBottom();

            // Right Border
            else if (MousePosition().X >= SystemParameters.VirtualScreenWidth - 50)
                CustomWindowSnapToRight();

            // Top
            else if (MousePosition().Y <= 10)
                CustomWindowSnapToTop();

            // Bottom
            else if (MousePosition().Y > workAreaScreen.Height - 50)
                CustomWindowSnapToBottom();
        }

        private void CustomWindowSnapToLeftTop()
        {
            var screen = System.Windows.SystemParameters.WorkArea;

            this.Width = screen.Width / 2;
            this.Height = screen.Height / 2;

            this.Left = 0;
            this.Top = 0;
        }

        private void CustomWindowSnapToLeftBottom()
        {
            var screen = System.Windows.SystemParameters.WorkArea;

            this.Width = screen.Width / 2;
            this.Height = screen.Height / 2;

            this.Left = 0;
            this.Top = screen.Height - Height;
        }

        private void CustomWindowSnapToLeft()
        {
            var screen = System.Windows.SystemParameters.WorkArea;

            this.Width = screen.Width / 2;
            this.Height = screen.Height;

            this.Top = 0;
            this.Left = 0;
        }

        private void CustomWindowSnapToRightTop()
        {
            var screen = System.Windows.SystemParameters.WorkArea;

            this.Width = screen.Width / 2;
            this.Height = screen.Height / 2;

            var virtualScreenWidth = System.Windows.SystemParameters.VirtualScreenWidth;
            var leftScreenEdge = screen.Left;
            var windowWidth = this.Width;

            this.Top = 0;
            this.Left = Math.Max(leftScreenEdge, leftScreenEdge + virtualScreenWidth - windowWidth);
        }

        private void CustomWindowSnapToRightBottom()
        {
            var screen = System.Windows.SystemParameters.WorkArea;

            this.Width = screen.Width / 2;
            this.Height = screen.Height / 2;

            var virtualScreenWidth = System.Windows.SystemParameters.VirtualScreenWidth;
            var leftScreenEdge = screen.Left;
            var windowWidth = this.Width;

            this.Top = screen.Height - Height;
            this.Left = Math.Max(leftScreenEdge, leftScreenEdge + virtualScreenWidth - windowWidth);
        }

        private void CustomWindowSnapToRight()
        {
            var screen = System.Windows.SystemParameters.WorkArea;

            this.Width = screen.Width / 2;
            this.Height = screen.Height;

            var virtualScreenWidth = System.Windows.SystemParameters.VirtualScreenWidth;
            var leftScreenEdge = screen.Left;
            var windowWidth = this.Width;

            this.Top = 0;
            this.Left = Math.Max(leftScreenEdge, leftScreenEdge + virtualScreenWidth - windowWidth);
        }
        private void CustomWindowSnapToTop()
        {
            var screen = System.Windows.SystemParameters.WorkArea;

            this.Width = screen.Width / 2;
            this.Height = screen.Height / 2;

            this.Left = (screen.Width - Width) / 2;
            this.Top = 0;
        }

        private void CustomWindowSnapToBottom()
        {
            var screen = System.Windows.SystemParameters.WorkArea;

            this.Width = screen.Width / 2;
            this.Height = screen.Height / 2;

            this.Left = (screen.Width - Width) / 2;
            this.Top = screen.Height - Height;
        }



        private void GridTopPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDraggingWindow)
            {
                System.Windows.Point currentPoint = e.GetPosition(this);
                this.Left += currentPoint.X - startPoint.X;
                this.Top += currentPoint.Y - startPoint.Y;
            }
        }

        private void BtnWindowMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;

        }

        private bool windowIsMaximized = false;
        private void BtnWindowMaximize_Click(object sender, RoutedEventArgs e)
        {
            CustomMaximizeWindow();
        }

        private void CustomMaximizeWindow()
        {
            if (windowIsMaximized == false)
            {
                this.WindowState = WindowState.Maximized;
                windowIsMaximized = true;
                BtnWindowMaximize.Content = "🗗";
            }
            else
            {
                this.WindowState = WindowState.Normal;
                windowIsMaximized = false;
                BtnWindowMaximize.Content = "🗖";
            }
        }

        private void BtnWindowClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        #endregion

        #region Drop down menu buttons
        private void DropDownButton_Click(object sender, RoutedEventArgs e)
        {
            CustomDropDown.IsOpen = true;
        }

        private void BtnGrayscale_Click(object sender, RoutedEventArgs e)
        {
            if (isProcessingFiles == false)
            {
                if (CbGrayscale.IsChecked == false)
                {
                    CbGrayscale.IsChecked = true;
                }
                else
                {
                    CbGrayscale.IsChecked = false;
                }
            }
            else
            {
                return;
            }
        }

        private void BtnPreserveFonts_Click(object sender, RoutedEventArgs e)
        {
            if (isProcessingFiles == false)
            {
                if (CbPrserveFonts.IsChecked == false)
                {
                    CbPrserveFonts.IsChecked = true;
                }
                else
                {
                    CbPrserveFonts.IsChecked = false;
                }
            }
            else
            {
                return;
            }
        }
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string statusReportOfCompressionPDF = string.Empty;
        public string StatusReportOfCompressionPDF
        {
            get { return statusReportOfCompressionPDF; }
            set
            {
                if (statusReportOfCompressionPDF != value)
                {
                    statusReportOfCompressionPDF = value;
                    OnPropertyChanged(nameof(StatusReportOfCompressionPDF));
                }
            }
        }

        public class PDFFileInfo : INotifyPropertyChanged
        {
            private string fileNamePDF = string.Empty;
            public string FileNamePDF
            {
                get { return fileNamePDF; }
                set
                {
                    if (fileNamePDF != value)
                    {
                        fileNamePDF = value;
                        OnPropertyChanged(nameof(FileNamePDF));
                    }
                }
            }

            private string fileOriginalSizePDF = string.Empty;
            public string FileOriginalSizePDF
            {
                get { return fileOriginalSizePDF; }
                set
                {
                    if (fileOriginalSizePDF != value)
                    {
                        fileOriginalSizePDF = value;
                        OnPropertyChanged(nameof(FileOriginalSizePDF));
                    }
                }
            }

            private string fileCompressedSizePDF = string.Empty;
            public string FileCompressedSizePDF
            {
                get { return fileCompressedSizePDF; }
                set
                {
                    if (fileCompressedSizePDF != value)
                    {
                        fileCompressedSizePDF = value;
                        OnPropertyChanged(nameof(FileCompressedSizePDF));
                    }
                }
            }

            private string fileCompressedRatePDF = string.Empty;
            public string FileCompressedRatePDF
            {
                get { return fileCompressedRatePDF; }
                set
                {
                    if (fileCompressedRatePDF != value)
                    {
                        fileCompressedRatePDF = value;
                        OnPropertyChanged(nameof(FileCompressedRatePDF));
                    }
                }
            }

            private string fileStatusReportPDF = string.Empty;
            public string FileStatusReportPDF
            {
                get { return fileStatusReportPDF; }
                set
                {
                    if (fileStatusReportPDF != value)
                    {
                        fileStatusReportPDF = value;
                        OnPropertyChanged(nameof(FileStatusReportPDF));
                    }
                }
            }

            public event PropertyChangedEventHandler? PropertyChanged;

            protected void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        #region Drag&Drop Handling
        private void DragAndDropArea_DragOver(object sender, DragEventArgs e)
        {
            // Prevent drag and drop if update is available
            if (isUpdateAvailable)
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                if (e.Data.GetData(DataFormats.FileDrop) is string[] data)
                {
                    // Check if any of the dragged items is a directory
                    bool isDirectory = data.Any(d => Directory.Exists(d));
                    if (!isDirectory)
                    {
                        // Only files, allow drop
                        e.Effects = DragDropEffects.Copy;
                    }
                    else
                    {
                        // Contains at least one directory, reject drop
                        e.Effects = DragDropEffects.None;
                    }
                }
                else
                {
                    // Data is null, reject drop
                    e.Effects = DragDropEffects.None;
                }
            }
            else
            {
                // Not file drop data, reject drop
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }
        #endregion

        #region total amount variables
        int totalAmountOfFiles = 0;
        int totalAmountOfProcesseFiles = 0;
        int totalAmountOfIgnoredFilesDueToSize = 0;
        int totalAmountOfIgnoredFilesDueToErrors = 0;
        int totalAmountOfIgnoredFilesDueToType = 0;
        long totalInputFileSizeBytes = 0;
        long totalOutFileSizeBytes = 0;
        #endregion

        #region Drag&Drop Event
        private async void DragAndDropArea_Drop(object sender, DragEventArgs e)
        {

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                foreach (string file in files)
                {
                    totalAmountOfFiles++;
                }

                isGrayscaleChecked = CbGrayscale.IsChecked ?? true;
                isPreserveFontsChecked = CbPrserveFonts.IsChecked ?? true;
                CbGrayscale.IsEnabled = false;
                CbPrserveFonts.IsEnabled = false;
                isProcessingFiles = true;

                foreach (string file in files)
                {
                    FileInfo fileInfo = new(file);

                    // Update the total file count and size
                    totalAmountOfProcesseFiles++;
                    totalInputFileSizeBytes += fileInfo.Length;

                    // Define the input file properties
                    string? inputFilePath = fileInfo.DirectoryName; // Path to the directory
                    string inputFileName = fileInfo.Name; // Full name with extension
                    string inputFileExtension = fileInfo.Extension;
                    long inputFileSizeBytes = fileInfo.Length;

                    // Create the "_compressed" directory if it doesn't exist
                    string outputDirectoryPath;
                    if (inputFilePath != null)
                    {
                        outputDirectoryPath = Path.Combine(inputFilePath, "_compressed");
                    }
                    else
                    {
                        outputDirectoryPath = "_compressed";
                    }
                    Directory.CreateDirectory(outputDirectoryPath);

                    // Define the output file properties
                    string outputFileName = SpecialCharacterConverter.ReplaceSpecialCharacters(inputFileName);
                    string outputFileNameWithPath = Path.Combine(outputDirectoryPath, outputFileName);

                    // Define a list of supported image extensions by ImageMagick
                    List<string> supportedExtensions = [".jpg", ".jpeg", ".png", ".tiff", ".tif", ".gif", ".bmp", ".webp", ".heic", ".psd"];

                    if (supportedExtensions.Contains(inputFileExtension.ToLower()))
                    {
                        await Task.Run(() => CompressImage(file, inputFileName, inputFileSizeBytes, outputDirectoryPath, outputFileNameWithPath));
                    }
                    else if (inputFileExtension.Equals(".pdf", StringComparison.CurrentCultureIgnoreCase))
                    {
                        await Task.Run(() => CompressPdf(file, inputFileName, inputFileSizeBytes, outputFileNameWithPath));
                    }
                    else
                    {
                        await Task.Run(() => HandleOtherFileTypes(file, inputFileName, inputFileSizeBytes, outputFileNameWithPath));
                    }

                    // Notify FileInfoListPDF that the list has been updated and generate StatusReport
                    await NotifyFileInfoListPdf();

                    // Update the total file count and size and calculate total compression rate in StatusReport
                    await UpdateStatusReport();
                }

                CbGrayscale.IsEnabled = true;
                CbPrserveFonts.IsEnabled = true;
                isProcessingFiles = false;
            }
        }
        #endregion

        #region File Compression of PDF
        private async Task CompressPdf(string file, string inputFileName, long inputFileSizeBytes, string outputFileNameWithPath)
        {
            try
            {
                MemoryStream ms = new();

                // Read the file content into a MemoryStream
                using (FileStream fs = new(file, FileMode.Open, FileAccess.Read))
                {
                    fs.CopyTo(ms);

                    // Reset the position of the MemoryStream to the beginning
                    ms.Position = 0;
                }

                // Remove the ISDOC image from the MemoryStream
                MemoryStream modifiedStream = AnnotationHelperClass.Remove(ms);

                // Pass the modified MemoryStream to the CompressFile method
                GhostscriptHelper.CompressFile(modifiedStream, outputFileNameWithPath, isGrayscaleChecked, isPreserveFontsChecked);

                // Dispose of the memory streams
                ms.Dispose();
                modifiedStream.Dispose();

                FileInfo outputFileInfo = new(outputFileNameWithPath);
                long outputFileSizeBytes = outputFileInfo.Length;
                if (CalculateCompressionRate(inputFileSizeBytes, outputFileSizeBytes) >= 100)
                {
                    await Task.Delay(100);
                    File.Copy(file, outputFileNameWithPath, true);
                    totalAmountOfIgnoredFilesDueToSize++;
                    await Task.Delay(100);
                    GetOutputFileCompressionStatusToList(inputFileName, inputFileSizeBytes, outputFileNameWithPath, "output size");
                }
                else
                {
                    GetOutputFileCompressionStatusToList(inputFileName, inputFileSizeBytes, outputFileNameWithPath, "");
                }
            }
            catch
            {
                await CopyUncompressedFileDueToError(file, inputFileName, inputFileSizeBytes, outputFileNameWithPath);
            }
        }
        #endregion

        #region File Compression of Images
        private async Task CompressImage(string file, string inputFileName, long inputFileSizeBytes, string outputDirectoryPath, string outputFileNameWithPath)
        {
            try
            {
                string outputFileNameWithPathWithExtensionPDF = Path.Combine(outputDirectoryPath, Path.GetFileNameWithoutExtension(outputFileNameWithPath) + ".pdf");

                // Convert the input image to a PDF and get the MemoryStream directly
                using (MemoryStream pdfMemoryStream = ImageToPdfConverter.ConvertImageToPdf(file, 150))
                {
                    // Ensure the MemoryStream's position is reset to the beginning.
                    pdfMemoryStream.Seek(0, SeekOrigin.Begin);

                    // Await the completion of the file compression task before exiting the using block.
                    GhostscriptHelper.CompressFile(pdfMemoryStream, outputFileNameWithPathWithExtensionPDF, isGrayscaleChecked, isPreserveFontsChecked);
                }

                // Now that the compression task is completed, it's safe to proceed with the next operation.
                GetOutputFileCompressionStatusToList(inputFileName, inputFileSizeBytes, outputFileNameWithPathWithExtensionPDF, "");
            }
            catch
            {
                await CopyUncompressedFileDueToError(file, inputFileName, inputFileSizeBytes, outputFileNameWithPath);
            }
        }
        #endregion

        #region Handle Other File Types
        private async Task HandleOtherFileTypes(string file, string inputFileName, long inputFileSizeBytes, string outputFileNameWithPath)
        {
            try
            {
                await CopyUncompressedFileDueToType(file, inputFileName, inputFileSizeBytes, outputFileNameWithPath);
            }
            catch
            {
            }
        }
        #endregion

        #region Handle Uncompressed File
        private async Task CopyUncompressedFileDueToType(string file, string inputFileName, long inputFileSizeBytes, string outputFileNameWithPath)
        {
            // Add a delay in ms
            await Task.Delay(100);

            // Update the total file count and size asynchronously
            totalAmountOfIgnoredFilesDueToType++;

            // Make a copy of the original file if compression fails asynchronously
            File.Copy(file, outputFileNameWithPath, true);

            // Add a delay in ms
            await Task.Delay(100);

            // Get the size of the output file into FileInfoListPDF asynchronously
            GetOutputFileCompressionStatusToList(inputFileName, inputFileSizeBytes, outputFileNameWithPath, "file type");
        }

        private async Task CopyUncompressedFileDueToError(string file, string inputFileName, long inputFileSizeBytes, string outputFileNameWithPath)
        {
            // Add a delay in ms
            await Task.Delay(100);

            // Update the total file count and size asynchronously
            totalAmountOfIgnoredFilesDueToErrors++;

            // Make a copy of the original file if compression fails asynchronously
            File.Copy(file, outputFileNameWithPath, true);

            // Add a delay in ms
            await Task.Delay(100);

            // Get the size of the output file into FileInfoListPDF asynchronously
            GetOutputFileCompressionStatusToList(inputFileName, inputFileSizeBytes, outputFileNameWithPath, "error");
        }

        #endregion

        #region Update UI
        private async Task UpdateStatusReport()
        {
            string totalInputFileSizeBytesAsString = await Task.Run(() => ConvertBytesToKilobytesOrMegabytes(totalInputFileSizeBytes));
            string totalOutFileSizeBytesAsString = await Task.Run(() => ConvertBytesToKilobytesOrMegabytes(totalOutFileSizeBytes));
            string totalCompressionRate = await Task.Run(() => CalculateCompressionRate(totalInputFileSizeBytes, totalOutFileSizeBytes).ToString()) + "%";

            string statusReport = $"Compressed: {totalAmountOfProcesseFiles}/{totalAmountOfFiles}, file size from: {totalInputFileSizeBytesAsString} to: {totalOutFileSizeBytesAsString} = {totalCompressionRate} of original file size. Uncompressed due to output size: {totalAmountOfIgnoredFilesDueToSize}, due to file type: {totalAmountOfIgnoredFilesDueToType}, due to error: {totalAmountOfIgnoredFilesDueToErrors}";

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                StatusReportOfCompressionPDF = statusReport;
            });
        }

        private async Task NotifyFileInfoListPdf()
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                if (FileInfoListPDF != null)
                {
                    ListViewPDF.ItemsSource = FileInfoListPDF;
                }
            });
        }

        public void AddPdfFile(PDFFileInfo pdfFile)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                FileInfoListPDF.Add(pdfFile);
            });
        }

        private void GetOutputFileCompressionStatusToList(string inputFileName, long inputFileSizeBytes, string outputFileNameWithPathWithExtenstionPDF, string fileStatusReportPDF)
        {
            FileInfo outputFileInfo = new(outputFileNameWithPathWithExtenstionPDF);
            long outputFileSizeBytes = outputFileInfo.Length;

            // Update the total output file size
            totalOutFileSizeBytes += outputFileSizeBytes;

            string inputFileSizeKB = ConvertBytesToKilobytesOrMegabytes(inputFileSizeBytes);
            string outputFileSizeKB = ConvertBytesToKilobytesOrMegabytes(outputFileSizeBytes);

            string compressionRate = CalculateCompressionRate(inputFileSizeBytes, outputFileSizeBytes).ToString() + "%";

            // Use the Dispatcher to update the ObservableCollection on the UI thread
            Application.Current.Dispatcher.Invoke(() =>
            {
                FileInfoListPDF.Add(new PDFFileInfo
                {
                    FileNamePDF = inputFileName,
                    FileOriginalSizePDF = inputFileSizeKB,
                    FileCompressedSizePDF = outputFileSizeKB,
                    FileCompressedRatePDF = compressionRate,
                    FileStatusReportPDF = fileStatusReportPDF
                });
            });
        }
        #endregion

        #region Calculation Methods
        private static int CalculateCompressionRate(long inputFileSizeBytes, long outputFileSizeBytes)
        {
            int compressionRate = (int)((outputFileSizeBytes * 100) / inputFileSizeBytes);
            return compressionRate;
        }

        private static string ConvertBytesToKilobytesOrMegabytes(long bytes)
        {
            const long kilobyte = 1024;
            //const long megabyte = 1024 * kilobyte;

            // Convert bytes to kilobytes (KB) - Windows-style calculation rounds down to the nearest whole number
            long kilobytes = (bytes + kilobyte - 1) / kilobyte;

            // If the file size is 1024KB or more, convert it to megabytes (MB) and format the string accordingly
            if (kilobytes >= 1024)
            {
                long megabytes = kilobytes / kilobyte;
                return megabytes + " MB";
            }
            else
            {
                // If the file size is less than 1024KB, format the string in KB
                return kilobytes + " KB";
            }
        }

        #endregion

        #region Licensing
        private void BtnAbout_Click(object sender, RoutedEventArgs e)
        {
            License licenseWindow = new()
            {
                Owner = Application.Current.MainWindow
            };

            licenseWindow.Left = Application.Current.MainWindow.Left + (Application.Current.MainWindow.Width - licenseWindow.Width) / 2;
            licenseWindow.Top = Application.Current.MainWindow.Top + (Application.Current.MainWindow.Height - licenseWindow.Height) / 2;

            licenseWindow.ShowDialog();
        }
        #endregion

        #region Update
        private async Task CheckForUpdatesAsync()
        {
            try
            {
                Version currentVersion = Update.GetCurrentVersion();
                Version latestVersion;

                try
                {
                    latestVersion = await Update.GetLatestReleaseVersion();
                }
                catch (HttpRequestException)
                {
                    // Silently handle no internet connection
                    return;
                }
                catch (Exception)
                {
                    // For any other network-related errors
                    return;
                }

                if (Update.IsUpdateAvailable(currentVersion, latestVersion))
                {
                    isUpdateAvailable = true;
                    UpdateWindow updateWindow = new UpdateWindow(this, currentVersion, latestVersion);
                    updateWindow.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.LogAndShowError(ex);
            }
        }
        #endregion
    }
}