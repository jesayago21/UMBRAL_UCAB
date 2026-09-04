FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/backend/Umbral.Gateway/Umbral.Gateway.csproj", "src/backend/Umbral.Gateway/"]
RUN dotnet restore "src/backend/Umbral.Gateway/Umbral.Gateway.csproj"

COPY . .
WORKDIR /src/src/backend/Umbral.Gateway
RUN dotnet publish "Umbral.Gateway.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

COPY --from=build /app/publish .

EXPOSE 8000
ENV ASPNETCORE_URLS=http://+:8000
ENTRYPOINT ["dotnet", "Umbral.Gateway.dll"]
