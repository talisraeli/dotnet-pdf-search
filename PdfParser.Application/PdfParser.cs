using SkiaSharp;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;
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
        PdfDocumentBuilder builder = new();
        List<int> pageNumbers = [];
        string[] searchedChars =
            searchedText
            .ToCharArray()
            .Select(c => c.ToString())
            .ToArray();

        foreach (Page page in document.GetPages())
        {
            var rectangles =
                GetMatchingWords(searchedChars, page.Letters)
                .GroupBy(w => w.BoundingBox.Top)
                .SelectMany(g => g)
                .Select(w => w.BoundingBox)
                .ToArray();

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

    private static Word[] GetMatchingWords(
        string[] searchedChars,
        IReadOnlyList<Letter> letters)
    {
        return [.. letters
            .Aggregate((
                lastIndex: -1,
                letterList: new List<Letter>(searchedChars.Length),
                wordList: new List<Word>()),
                (acc, letter) =>
                    AggregateLetters(searchedChars, acc, letter))
            .wordList];
    }

    private static (int, List<Letter>, List<Word>) AggregateLetters(
        string[] searchedChars,
        (int lastIndex, List<Letter> letterList, List<Word> wordList) acc,
        Letter letter)
    {
        if (!letter.Value.Equals(
        searchedChars[acc.lastIndex + 1],
        StringComparison.OrdinalIgnoreCase))
        {
            acc.lastIndex = -1;
            acc.letterList.Clear();
            return acc;
        }

        if (acc.letterList.LastOrDefault() != null &&
            acc.letterList.Last().EndBaseLine.Y != letter.StartBaseLine.Y)
        {
            acc.wordList.Add(new(acc.letterList));
            acc.letterList.Clear();
        }

        acc.lastIndex++;
        acc.letterList.Add(letter);

        if (acc.lastIndex == searchedChars.Length - 1)
        {
            acc.wordList.Add(new(acc.letterList));
            acc.lastIndex = -1;
            acc.letterList.Clear();
        }

        return acc;
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
            pageBuilder.DrawLine(
                rectangle.BottomLeft.Translate(0, -2),
                rectangle.BottomRight.Translate(0, -2),
                2);
        }
    }

    private sealed record ParsedPdfResult(
        byte[] Data,
        int[] PageNumbers);
}
