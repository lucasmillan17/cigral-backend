# Imagen de la API de Cigral. Ver CigralBackend/docs/DEPLOY_DOCKER.md

# --- Build ---
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Primero solo los .csproj para cachear el restore entre builds
COPY CigralBackend/CigralBackend.Api.csproj CigralBackend/
COPY CigralBack.Api/CigralBackend.Application.csproj CigralBack.Api/
COPY CigralBackend.Domain/CigralBackend.Domain.csproj CigralBackend.Domain/
COPY CigralBackend.Infraestructure/CigralBackend.Infraestructure.csproj CigralBackend.Infraestructure/
RUN dotnet restore CigralBackend/CigralBackend.Api.csproj

COPY . .
RUN dotnet publish CigralBackend/CigralBackend.Api.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false

# --- Runtime ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

# libfontconfig1: QuestPDF (SkiaSharp) la necesita para generar PDFs en Linux
# tzdata: el código usa DateTime.Now, así que el contenedor tiene que estar en hora argentina
RUN apt-get update \
    && apt-get install -y --no-install-recommends libfontconfig1 tzdata \
    && rm -rf /var/lib/apt/lists/*

ENV TZ=America/Argentina/Buenos_Aires \
    ASPNETCORE_HTTP_PORTS=8080 \
    ASPNETCORE_ENVIRONMENT=Production

WORKDIR /app
COPY --from=build /app/publish .

USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "CigralBackend.Api.dll"]
