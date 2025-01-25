using System.IO;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace CompressPDF
{
    public static class AnnotationHelperClass
    {
        /// <summary>
        /// Removes all annotations from a PDF document
        /// </summary>
        /// <param name="inputStream">Input PDF memory stream</param>
        /// <returns>Memory stream with annotations removed</returns>
        public static MemoryStream Remove(MemoryStream? inputStream)
        {
            if (inputStream == null || inputStream.Length == 0)
            {
                return new MemoryStream();
            }

            MemoryStream outputStream = new MemoryStream();

            try
            {
                // Open the input PDF document
                using (PdfDocument inputDocument = PdfReader.Open(inputStream, PdfDocumentOpenMode.Import))
                {
                    // Create a new document
                    using (PdfDocument outputDocument = new PdfDocument())
                    {
                        // Copy pages without annotations
                        for (int i = 0; i < inputDocument.Pages.Count; i++)
                        {
                            PdfPage importedPage = inputDocument.Pages[i];
                            PdfPage newPage = outputDocument.AddPage(importedPage);

                            // Explicitly clear annotations
                            if (newPage.Annotations != null)
                            {
                                newPage.Annotations.Clear();
                            }
                        }

                        // Save the new document to the output stream
                        outputDocument.Save(outputStream);
                    }
                }

                // Reset the position of the output stream
                outputStream.Position = 0;
                return outputStream;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception in PDF annotation removal: {ex.Message}");

                // Return original stream if processing fails
                inputStream.Position = 0;
                return inputStream;
            }
        }
    }
}