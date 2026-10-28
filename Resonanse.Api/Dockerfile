# ---------- build ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Resonanse.sln ./
COPY Resonanse.Api/Resonanse.Api.csproj Resonanse.Api/
COPY Resonanse.Application/Resonanse.Application.csproj Resonanse.Application/
COPY Resonanse.Domain/Resonanse.Domain.csproj Resonanse.Domain/
COPY Resonanse.Infrastructure/Resonanse.Infrastructure.csproj Resonanse.Infrastructure/

RUN dotnet restore Resonanse.Api/Resonanse.Api.csproj

COPY . .
RUN dotnet publish Resonanse.Api/Resonanse.Api.csproj \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

# ---------- runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

RUN apt-get update && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

USER app

ENTRYPOINT ["dotnet", "Resonanse.Api.dll"]
