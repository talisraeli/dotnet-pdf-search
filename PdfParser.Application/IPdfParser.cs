namespace PdfParser.Application;

public interface IPdfParser
{
    public ParsedPdfPage[] SearchPdfForText(Stream pdf, string searchedText);
}
