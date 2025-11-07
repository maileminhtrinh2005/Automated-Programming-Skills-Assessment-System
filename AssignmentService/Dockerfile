#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.




FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["AssignmentService/AssignmentService.csproj", "AssignmentService/"]
COPY ["ShareLibrary/ShareLibrary.csproj", "ShareLibrary/"]
RUN dotnet restore "./AssignmentService/AssignmentService.csproj"
COPY . .


WORKDIR "/src/AssignmentService"
RUN dotnet build "./AssignmentService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./AssignmentService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false


FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=publish /app/publish .



ENTRYPOINT ["dotnet", "AssignmentService.dll"]