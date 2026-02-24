using Azure;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace RagDemo.Core;

public class PdfIngester
{
    private readonly DocumentIngester _ingester;

    public PdfIngester(DocumentIngester ingester)
    {
        _ingester = ingester;
    }

    public async Task IngestPdfAsync(string filePath)
    {
        var fileName = Path.GetFileNameWithoutExtension(filePath);
        Console.WriteLine($"Reading {fileName}...");

        var text = ExtractText(filePath);
        await _ingester.IngestTextAsync(text, fileName);
    }

    public async Task IngestFolderAsync(string folderPath)
    {
        var pdfs = Directory.GetFiles(folderPath, "*.pdf");
        Console.WriteLine($"Found {pdfs.Length} PDFs...\n");

        foreach (var pdf in pdfs)
        {
            await IngestPdfAsync(pdf);
        }
    }

    private string ExtractText(string filePath)
    {
        using var document = PdfDocument.Open(filePath);
        var sb = new System.Text.StringBuilder();

        foreach (Page page in document.GetPages())
        {
            sb.AppendLine(page.Text);
        }

        return sb.ToString();
    }
}