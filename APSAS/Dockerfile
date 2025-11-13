# ===== STAGE 1: Build =====
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy file csproj of service and share library, cause library cannot run independence
COPY APSAS/APSAS.csproj APSAS/
RUN dotnet restore APSAS/APSAS.csproj

# Copy all source code of service
COPY . .

# Build and publish
WORKDIR "/src/APSAS"
RUN dotnet publish -c Release -o /app/publish

# ===== STAGE 2: Runtime =====
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app/APSAS

# Copy publish to runtime image
COPY --from=build /app/publish .  



EXPOSE 5261

ENTRYPOINT ["dotnet", "APSAS.dll"]

