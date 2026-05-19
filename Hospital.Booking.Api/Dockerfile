FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

RUN useradd -m appuser
USER appuser

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Hospital.Booking.Api/Hospital.Booking.Api.csproj", "Hospital.Booking.Api/"]
RUN dotnet restore "Hospital.Booking.Api/Hospital.Booking.Api.csproj"

COPY . .
WORKDIR "/src/Hospital.Booking.Api"
RUN dotnet publish "Hospital.Booking.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Hospital.Booking.Api.dll"]