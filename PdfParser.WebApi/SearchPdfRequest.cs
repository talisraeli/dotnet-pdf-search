namespace PdfParser.WebApi;

internal class SearchPdfRequest
{
    public required IFormFile Pdf { get; set; }

    public required string SearchedText { get; set; }
}
