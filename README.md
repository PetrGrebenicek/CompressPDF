# CompressPDF

Portable application with drag and drop function that:
Compresses the PDF files using GhostScript
Converts images into PDF using ImageMagic and imidiatly compresses the PDF file using GhostScript
To resolve known issue PDFSharp is removing annotation of ISDOC as it overlapped over the content in output PDF

Offers options to outup in a gray scale for better results (default value ON) and option to preserve fonts in case that output PDF file has corrupted fonts at cost of bigger output file size (default value OFF)

Outputs into new folder _compressed to prevent loss due to data corruption in process
In case of error during converting/compressing process the original file is copied into output folder
If the file is not PDF or Image it is copied into outpud folder
