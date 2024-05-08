#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:9.0-preview AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0-preview AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Packages.props", "."]
COPY ["Directory.Build.props", "."]
COPY ["PdfParser.WebApi/PdfParser.WebApi.csproj", "PdfParser.WebApi/"]
COPY ["PdfParser.Application/PdfParser.Application.csproj", "PdfParser.Application/"]
RUN dotnet restore "./PdfParser.WebApi/PdfParser.WebApi.csproj"
COPY . .
WORKDIR "/src/PdfParser.WebApi"
RUN dotnet build "./PdfParser.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./PdfParser.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PdfParser.WebApi.dll"]