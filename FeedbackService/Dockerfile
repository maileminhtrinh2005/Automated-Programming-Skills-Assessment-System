# ===== STAGE 1: Build =====
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy file csproj of service and share library, cause library cannot run independence
COPY FeedbackService/FeedbackService.csproj FeedbackService/
COPY ShareLibrary/ShareLibrary.csproj ShareLibrary/
RUN dotnet restore FeedbackService/FeedbackService.csproj

# Copy all source code of service
COPY . .

# Build and publish
WORKDIR "/src/FeedbackService"
RUN dotnet publish -c Release -o /app/publish

# ===== STAGE 2: Runtime =====
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app/FeedbackService

# Copy publish to runtime image
COPY --from=build /app/publish .  



EXPOSE 5064

ENTRYPOINT ["dotnet", "FeedbackService.dll"]

