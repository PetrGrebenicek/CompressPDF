using System.IO;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using PdfSharp.Pdf.Annotations;

namespace CompressPDF
{
    public static class IsdocHelper
    {
        #region Remove all annotations
        public static MemoryStream Remove(MemoryStream inputStream)
        {
            MemoryStream outputStream = new();

            try
            {
                // Load the input PDF document
                PdfDocument document = PdfReader.Open(inputStream, PdfDocumentOpenMode.Modify);

                // Iterate through each page
                foreach (var page in document.Pages)
                {
                    // Check for annotations on the page
                    if (page.Annotations?.Count > 0)
                    {
                        // Clear all annotations
                        page.Annotations.Clear();
                    }
                }

                // Save the modified document to the memory stream
                document.Save(outputStream);

                // Reset the position of the output stream to the beginning
                outputStream.Position = 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception: {ex.Message}");
                return inputStream;
            }

            return outputStream;
        }
        #endregion
    }
}