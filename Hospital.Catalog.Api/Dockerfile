FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

RUN useradd -m appuser
USER appuser

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Hospital.Catalog.Api/Hospital.Catalog.Api.csproj", "Hospital.Catalog.Api/"]
RUN dotnet restore "Hospital.Catalog.Api/Hospital.Catalog.Api.csproj"

COPY . .
WORKDIR "/src/Hospital.Catalog.Api"
RUN dotnet publish "Hospital.Catalog.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Hospital.Catalog.Api.dll"]