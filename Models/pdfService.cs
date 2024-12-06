using SelectPdf;
using System;
using System.IO;

public interface IPdfService
{
    byte[] ConvertHtmlToPdf(string htmlContent);
}

public class PdfService : IPdfService
{
    public byte[] ConvertHtmlToPdf(string htmlContent)
    {
        try
        {
            // Create a new instance of HtmlToPdf
            var converter = new HtmlToPdf();

            // Convert the HTML string to PDF
            PdfDocument document = converter.ConvertHtmlString(htmlContent);

            // Return the PDF as a byte array
            byte[] pdfBytes = document.Save();
            document.Close(); // Ensure the document is properly closed and resources are freed

            return pdfBytes;
        }
        catch (Exception ex)
        {
            // Log the exception (you can use your logging mechanism here)
            Console.WriteLine($"Error converting HTML to PDF: {ex.Message}");
            return null; // Return null in case of an error (you can handle it based on your needs)
        }
    }
}
