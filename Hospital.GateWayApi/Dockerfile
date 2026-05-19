FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

RUN useradd -m appuser
USER appuser

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Hospital.GateWayApi/Hospital.GateWayApi.csproj", "Hospital.GateWayApi/"]
RUN dotnet restore "Hospital.GateWayApi/Hospital.GateWayApi.csproj"

COPY . .
WORKDIR "/src/Hospital.GateWayApi"
RUN dotnet publish "Hospital.GateWayApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Hospital.GateWayApi.dll"]