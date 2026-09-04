FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/backend/Umbral.API/Umbral.API.csproj", "src/backend/Umbral.API/"]
COPY ["src/backend/Umbral.Application/Umbral.Application.csproj", "src/backend/Umbral.Application/"]
COPY ["src/backend/Umbral.Domain/Umbral.Domain.csproj", "src/backend/Umbral.Domain/"]
COPY ["src/backend/Umbral.Infrastructure/Umbral.Infrastructure.csproj", "src/backend/Umbral.Infrastructure/"]
RUN dotnet restore "src/backend/Umbral.API/Umbral.API.csproj"

COPY . .
WORKDIR /src/src/backend/Umbral.API
RUN dotnet publish "Umbral.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

COPY --from=build /app/publish .

EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000
ENTRYPOINT ["dotnet", "Umbral.API.dll"]
