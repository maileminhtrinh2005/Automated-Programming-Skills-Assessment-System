# ===== STAGE 1: Build =====
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy file csproj of service and share library, cause library cannot run independence
COPY AssignmentService/AssignmentService.csproj AssignmentService/
COPY ShareLibrary/ShareLibrary.csproj ShareLibrary/
RUN dotnet restore AssignmentService/AssignmentService.csproj

# Copy all source code of service
COPY . .

# Build and publish
WORKDIR "/src/AssignmentService"
RUN dotnet publish -c Release -o /app/publish

# ===== STAGE 2: Runtime =====
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS runtime
WORKDIR /app/AssignmentService

# Copy publish to runtime image
COPY --from=build /app/publish .  



EXPOSE 5267

ENTRYPOINT ["dotnet", "AssignmentService.dll"]

