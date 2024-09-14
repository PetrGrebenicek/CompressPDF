﻿using ImageMagick;
using System.IO;

namespace CompressPDF
{
    public class ImageToPdfConverter
    {
        public static MemoryStream ConvertImageToPdf(string inputImagePath, double dpi)
        {
            try
            {
                using var image = new MagickImage(inputImagePath);
                // A4 size in inches
                double a4WidthInInches = 8.27;
                double a4HeightInInches = 11.69;

                // Convert A4 dimensions to pixels at the specified DPI
                double a4WidthInPixels = a4WidthInInches * dpi;
                double a4HeightInPixels = a4HeightInInches * dpi;

                // Determine the scaling factor needed to fit the image within A4 dimensions
                double scalingFactor = Math.Min(a4WidthInPixels / image.Width, a4HeightInPixels / image.Height);

                // Calculate the new dimensions
                int newWidth = (int)(image.Width * scalingFactor);
                int newHeight = (int)(image.Height * scalingFactor);

                // Resize the image to fit within A4 dimensions
                image.Resize(newWidth, newHeight);

                // Change the image density to reflect the new DPI settings
                image.Density = new Density(dpi, dpi);

                // Explicitly set the format to PDF to ensure correct conversion
                image.Format = MagickFormat.Pdf;

                // Set the page size to A4 at the specified DPI, this will center the image on an A4 page
                double a4WidthInPoints = a4WidthInPixels / dpi * 72;
                double a4HeightInPoints = a4HeightInPixels / dpi * 72;
                image.Settings.SetDefine(MagickFormat.Pdf, "page", $"{a4WidthInPoints}x{a4HeightInPoints}");

                // Create a MemoryStream to store the PDF data
                MemoryStream pdfStream = new();

                // Write the image data to the MemoryStream
                image.Write(pdfStream);

                // Reset the MemoryStream position for reading
                pdfStream.Position = 0;

                return pdfStream;
            }
            catch (Exception ex)
            {
                throw new Exception("Error converting image to PDF: " + ex.Message);
            }
        }
    }
}