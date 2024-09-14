using System.Runtime.InteropServices;
using System.IO;

namespace CompressPDF
{
    public static partial class GhostscriptHelper
    {
        // Initializes Ghostscript DLL
        [LibraryImport("gsdll64.dll", EntryPoint = "gsapi_new_instance")]
        private static partial int gsapi_new_instance(out IntPtr pinstance, IntPtr caller_handle);

        [LibraryImport("gsdll64.dll", EntryPoint = "gsapi_init_with_args")]
        private static partial int gsapi_init_with_args(IntPtr instance, int argc, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr)] string[] argv);

        [LibraryImport("gsdll64.dll", EntryPoint = "gsapi_exit")]
        private static partial int gsapi_exit(IntPtr instance);

        public static void CompressFile(MemoryStream inputFileStream, string outputFileName, bool grayScaleMode)
        {
            int result = gsapi_new_instance(out nint instance, IntPtr.Zero);
            if (result != 0)
            {
                throw new Exception("Error initializing Ghostscript instance");
            }

            // Save the input stream to a temporary file
            string tempFilePath = Path.GetTempFileName();
            using (FileStream fileStream = new(tempFilePath, FileMode.Create))
            {
                inputFileStream.Position = 0; // Reset stream position
                inputFileStream.CopyTo(fileStream);
            }

            // Define ghostscript arguments for color mode and grayscale mode
            string[] arguments;

            if (grayScaleMode)
            {
                arguments = [
                    "-q", "-dNOPAUSE", "-dBATCH", "-dSAFER",
                    "-sDEVICE=pdfwrite",
                    "-dPDFSETTINGS=/ebook",
                    "-sProcessColorModel=DeviceRGB",
                    "-sColorConversionStrategy=Gray",
                    $"-sOutputFile={outputFileName}",
                    tempFilePath
                ];
            }
            else
            {
                arguments = [
                    "-q", "-dNOPAUSE", "-dBATCH", "-dSAFER",
                    "-sDEVICE=pdfwrite",
                    "-dPDFSETTINGS=/ebook",
                    "-sProcessColorModel=DeviceRGB",
                    $"-sOutputFile={outputFileName}",
                    tempFilePath
                ];
            }

            // Initialize Ghostscript with arguments
            result = gsapi_init_with_args(instance, arguments.Length, arguments);
            if (result != 0)
            {
                gsapi_exit(instance);
                throw new Exception("Error during compression");
            }

            // Exit Ghostscript instance to release resources
            result = gsapi_exit(instance);
            if (result != 0)
            {
                throw new Exception("Error exiting Ghostscript instance");
            }

            // Clean up temporary file
            File.Delete(tempFilePath);

            // Check output file size to ensure compression was successful
            if (new FileInfo(outputFileName).Length == 0)
            {
                throw new Exception("Output file is 0 bytes. Compression may have failed.");
            }
        }
    }
}