using Microsoft.AspNetCore.Mvc;
using PdfParser.Application;

namespace PdfParser.WebApi;

public static class WebApplicationExtensions
{
    public static WebApplication MapRoutes(this WebApplication app)
    {
        app.MapPost("/search-pdf", Handle)
            .DisableAntiforgery();

        return app;
    }

    private static IResult Handle(
        [FromForm] IFormFile pdf,
        [FromForm] string searchedText,
        IPdfParser pdfParser)
    {
        ParsedPdfPage[] pages = pdfParser.SearchPdfForText(
            pdf.OpenReadStream(),
            searchedText);

        if (pages.Length == 0)
        {
            return Results.NotFound();
        }

        if (pages.Length == 1)
        {
            return Results.File(pages[0].Data, "image/png", $"page-{pages[0].PageNumber}.png");
        }

        return Results.Ok(new
        {
            UrlToConvertPngs = "https://base64.guru/converter/decode/image/png",
            Pages = pages.ToDictionary(
                p => $"page-{p.PageNumber}.png",
                p => GetBase64StringFromStream(p.Data))
        });
    }

    private static string GetBase64StringFromStream(Stream data)
    {
        data.Position = 0;
        byte[] bytes = new byte[data.Length];
        data.Read(bytes, 0, bytes.Length);
        return Convert.ToBase64String(bytes);
    }
}
