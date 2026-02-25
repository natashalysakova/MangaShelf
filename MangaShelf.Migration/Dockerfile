# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy all csproj files first to leverage Docker layer caching for restore
COPY ["MangaShelf.Parser/MangaShelf.Parser.csproj", "MangaShelf.Parser/"]
COPY ["MangaShelf.Migration/MangaShelf.Migration.csproj", "MangaShelf.Migration/"]
COPY ["MangaShelf.DAL/MangaShelf.DAL.csproj", "MangaShelf.DAL/"]
COPY ["MangaShelf.Infrastructure/MangaShelf.Infrastructure.csproj", "MangaShelf.Infrastructure/"]
COPY ["MangaShelf.BL/MangaShelf.BL.csproj", "MangaShelf.BL/"]
COPY ["MangaShelf.DAL.Identity/MangaShelf.DAL.Identity.csproj", "MangaShelf.DAL.Identity/"]
COPY ["MangaShelf.DAL.System/MangaShelf.DAL.System.csproj", "MangaShelf.DAL.System/"]
COPY ["MangaShelf.Common/MangaShelf.Common.csproj", "MangaShelf.Common/"]

# Restore dependencies - this layer will be cached unless csproj files change
RUN dotnet restore "MangaShelf.Migration/MangaShelf.Migration.csproj"

# Copy the rest of the source code
COPY . .

WORKDIR "/src/MangaShelf.Migration"
RUN dotnet build "./MangaShelf.Migration.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./MangaShelf.Migration.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MangaShelf.Migration.dll"]