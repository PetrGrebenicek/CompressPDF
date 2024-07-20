using System.Runtime.InteropServices;
using System.IO;

namespace CompressPDF
{
    public class GhostscriptHelper
    {

        // Initializes Ghostscript DLL
        [DllImport("gsdll64.dll", EntryPoint = "gsapi_new_instance")]
        private static extern int gsapi_new_instance(out IntPtr pinstance, IntPtr caller_handle);

        [DllImport("gsdll64.dll", EntryPoint = "gsapi_init_with_args")]
        private static extern int gsapi_init_with_args(IntPtr instance, int argc, string[] argv);

        [DllImport("gsdll64.dll", EntryPoint = "gsapi_exit")]
        private static extern int gsapi_exit(IntPtr instance);

        /// <summary>
        /// Compresses a file using Ghostscript
        /// </summary>
        /// <param name="inputFileStream"></param>
        /// <param name="outputFileName"></param>
        /// <exception cref="Exception"></exception>
        public static void CompressFile(MemoryStream inputFileStream, string outputFileName, bool grayScaleMode)
        {
            IntPtr instance;
            int result = gsapi_new_instance(out instance, IntPtr.Zero);
            if (result != 0)
            {
                throw new Exception("Error initializing Ghostscript instance");
            }

            // Save the input stream to a temporary file
            string tempFilePath = Path.GetTempFileName();
            using (FileStream fileStream = new FileStream(tempFilePath, FileMode.Create))
            {
                inputFileStream.Position = 0; // Reset stream position
                inputFileStream.CopyTo(fileStream);
            }

            string[] arguments;

            if (grayScaleMode == true)
            {
                arguments = new string[] {
                "-q", "-dNOPAUSE", "-dBATCH", "-dSAFER",
                "-sDEVICE=pdfwrite",
                "-dPDFSETTINGS=/ebook",
                "-sProcessColorModel=DeviceGray",
                "-sColorConversionStrategy=Gray",
                $"-sOutputFile={outputFileName}",
                tempFilePath
                };
            }
            else
            {
                arguments = new string[] {
                "-q", "-dNOPAUSE", "-dBATCH", "-dSAFER",
                "-sDEVICE=pdfwrite",
                "-dPDFSETTINGS=/ebook",
                "-sProcessColorModel=DeviceGray",
                $"-sOutputFile={outputFileName}",
                tempFilePath
               };
            }

            // Define Ghostscript arguments


            // Initialize Ghostscript with arguments
            result = gsapi_init_with_args(instance, arguments.Length, arguments);
            if (result != 0)
            {
                gsapi_exit(instance);
                throw new Exception("Error during compression");
            }

            // Exit Ghostscript instance to release resources
            gsapi_exit(instance);

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