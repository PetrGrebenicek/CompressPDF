using System.IO;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using PdfSharp.Pdf.Annotations;

namespace CompressPDF
{
    public static class IsdocHelper
    {
        #region Remove annotation by author <FEFF004900530044004F0043> = "þÿISDOC"
        public static MemoryStream Remove(MemoryStream inputStream)
        {
            MemoryStream outputStream = new();
            string targetAuthor = "<FEFF004900530044004F0043>"; // "þÿISDOC" in hex

            try
            {
                // Load the input PDF document
                PdfDocument document = PdfReader.Open(inputStream, PdfDocumentOpenMode.Modify);

                // Iterate through each page
                foreach (var page in document.Pages)
                {
                    // List of annotations to remove
                    var annotationsToRemove = new List<PdfAnnotation>();

                    // Check for annotations on the page
                    if (page.Annotations?.Count > 0)
                    {
                        foreach (var item in page.Annotations)
                        {
                            // Cast item to PdfAnnotation
                            if (item is PdfAnnotation annotation)
                            {
                                // Check if the annotation has an Author key
                                if (annotation.Elements.ContainsKey("/T"))
                                {
                                    string authorRaw = annotation.Elements["/T"]?.ToString() ?? string.Empty;

                                    // Check if the annotation is created by the target author
                                    if (!string.IsNullOrEmpty(authorRaw) && authorRaw.Contains(targetAuthor))
                                    {
                                        // Mark the annotation for removal
                                        annotationsToRemove.Add(annotation);
                                    }
                                }
                            }
                        }

                        // Remove the marked annotations
                        foreach (var annotation in annotationsToRemove)
                        {
                            page.Annotations.Remove(annotation);
                        }
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
                // Optionally, rethrow or handle the exception as necessary
            }

            return outputStream;
        }
        #endregion
    }
}