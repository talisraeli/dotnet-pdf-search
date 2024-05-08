namespace PdfParser.Application;

public sealed record ParsedPdfPage(
    int PageNumber,
    Stream Data);
