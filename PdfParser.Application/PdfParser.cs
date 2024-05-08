using SkiaSharp;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.DocumentLayoutAnalysis;
using UglyToad.PdfPig.DocumentLayoutAnalysis.PageSegmenter;
using UglyToad.PdfPig.Writer;

namespace PdfParser.Application;

internal sealed class PdfParser : IPdfParser
{
    [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "Supports the relevant platforms")]
    public ParsedPdfPage[] SearchPdfForText(Stream pdf, string searchedText)
    {
        ParsedPdfResult? parsedPdf = GetParsedPdf(pdf, searchedText);

        if (parsedPdf is null)
        {
            return [];
        }

        IEnumerable<SKBitmap> images = PDFtoImage.Conversion.ToImages(parsedPdf.Data);
        List<ParsedPdfPage> pages = [];

        foreach ((int Index, SKBitmap Data) image in images.Index())
        {
            ParsedPdfPage page = new(
                parsedPdf.PageNumbers[image.Index],
                image.Data
                    .Encode(SKEncodedImageFormat.Png, 100)
                    .AsStream());

            pages.Add(page);
        }

        return [.. pages];
    }

    private static ParsedPdfResult? GetParsedPdf(Stream pdf, string searchedText)
    {
        using PdfDocument document = PdfDocument.Open(pdf);
        SearchValues<string> searchValues = SearchValues.Create(
            searchedText.Split(), StringComparison.OrdinalIgnoreCase);
        PdfDocumentBuilder builder = new();
        List<int> pageNumbers = [];

        foreach (Page page in document.GetPages())
        {
            PdfRectangle[] rectangles = GetRectangles(page, searchedText, searchValues);

            if (rectangles.Length == 0)
            {
                continue;
            }

            pageNumbers.Add(page.Number);
            AddUnderlines(document, builder, page, rectangles);
        }

        if (builder.Pages.Count == 0)
        {
            return null;
        }

        return new(builder.Build(), [.. pageNumbers]);
    }

    private static void AddUnderlines(
        PdfDocument document,
        PdfDocumentBuilder builder,
        Page page,
        PdfRectangle[] rectangles)
    {
        PdfPageBuilder pageBuilder = builder.AddPage(document, page.Number);
        pageBuilder.SetStrokeColor(255, 0, 0);

        foreach (var rectangle in rectangles)
        {
            pageBuilder.DrawLine(rectangle.BottomLeft.Translate(-2, -2), rectangle.BottomRight.Translate(2, -2), 2);
        }
    }

    private static PdfRectangle[] GetRectangles(
        Page page,
        string searchedText,
        SearchValues<string> searchValues)
    {
        IEnumerable<Word> words = page.GetWords();
        IReadOnlyList<TextBlock> blocks = DocstrumBoundingBoxes.Instance.GetBlocks(words);

        return blocks
            .Where(b => b.Text
                .Contains(searchedText, StringComparison.OrdinalIgnoreCase))
            .SelectMany(tb => tb.TextLines)
            .SelectMany(tl => tl.Words)
            .Select(w => w.Text
                .SplitHyphens()
                .Select(h => h.Length)
                .Aggregate(
                    (List: new List<Word>(), LastIndex: 0),
                    (acc, i) => {
                        acc.List.Add(
                            new(w.Letters
                                .Skip(acc.LastIndex)
                                .Take(i)
                                .ToList()));
                        acc.LastIndex += i + 1;
                        return acc;
                    }))
            .SelectMany(a => a.List)
            .Where(w => searchValues
                .Contains(w.Text.RemoveSymbols()))
            .Select(w => w.BoundingBox)
            .ToArray();
    }

    private sealed record ParsedPdfResult(
        byte[] Data,
        int[] PageNumbers);
}
