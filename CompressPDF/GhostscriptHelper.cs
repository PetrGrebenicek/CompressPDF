using System.Runtime.InteropServices;
using System.IO;

namespace CompressPDF
{
    public static partial class GhostscriptHelper
    {
        #region Initializes Ghostscript DLL
        [LibraryImport("gsdll64.dll", EntryPoint = "gsapi_new_instance")]
        private static partial int gsapi_new_instance(out IntPtr pinstance, IntPtr caller_handle);

        [LibraryImport("gsdll64.dll", EntryPoint = "gsapi_init_with_args")]
        private static partial int gsapi_init_with_args(IntPtr instance, int argc, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr)][In] string[] argv);

        [LibraryImport("gsdll64.dll", EntryPoint = "gsapi_exit")]
        private static partial int gsapi_exit(IntPtr instance);
        #endregion

        #region Compress File
        public static void CompressFile(MemoryStream inputFileStream, string outputFileName, bool grayScaleMode, bool preserveFontsMode)
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
            List<string> arguments =
            [
                "-q", "-dNOPAUSE", "-dBATCH", "-dSAFER",
                "-sDEVICE=pdfwrite",
                // maximum compression, while maintaining quality
                "-dPDFSETTINGS=/ebook",
                "-dCompatibilityLevel=2.0",
                "-dDetectDuplicateImages=true",
                "-dDoNumCopies"
            ];
            if (grayScaleMode)
            {
                arguments.Add("-sProcessColorModel=DeviceGray");
                arguments.Add("-sColorConversionStrategy=Gray");
            }
            else
            {
                arguments.Add("-sProcessColorModel=DeviceRGB");
                arguments.Add("-sColorConversionStrategy=RGB");
            }
            if (preserveFontsMode)
            {
                arguments.Add("-dNoOutputFonts");
            }
            arguments.Add($"-sOutputFile={outputFileName}");
            arguments.Add(tempFilePath);

            // Convert the list back to an array
            string[] argumentsArray = [.. arguments];

            // Initialize Ghostscript with arguments
            result = gsapi_init_with_args(instance, argumentsArray.Length, argumentsArray);
            if (result != 0)
            {
                // Exit Ghostscript instance to release resources (ignore the result)
                _ = gsapi_exit(instance);
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
        #endregion
    }
}