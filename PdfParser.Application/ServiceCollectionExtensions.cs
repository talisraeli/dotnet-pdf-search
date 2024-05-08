using Microsoft.Extensions.DependencyInjection;

namespace PdfParser.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPdfParser(
        this IServiceCollection services)
    {
        services.AddSingleton<IPdfParser, PdfParser>();

        return services;
    }
}
