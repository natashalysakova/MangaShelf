# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# AMD64-specific base stage: installs Google Chrome
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base-amd64
RUN apt-get update && apt-get install curl gnupg -y \
    && curl --location --silent https://dl-ssl.google.com/linux/linux_signing_key.pub | apt-key add - \
    && sh -c 'echo "deb [arch=amd64] http://dl.google.com/linux/chrome/deb/ stable main" >> /etc/apt/sources.list.d/google.list' \
    && apt-get update \
    && apt-get install google-chrome-stable -y --no-install-recommends \
    && rm -rf /var/lib/apt/lists/*
ENV PUPPETEER_EXECUTABLE_PATH=/usr/bin/google-chrome

# ARM64-specific base stage: installs Chromium from Debian repos
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base-arm64
RUN apt-get update && apt-get install -y chromium --no-install-recommends \
    fonts-ipafont-gothic fonts-wqy-zenhei fonts-thai-tlwg fonts-kacst fonts-freefont-ttf \
    && rm -rf /var/lib/apt/lists/*
# Find the chromium binary (path varies by distro) and create a symlink at a known location
RUN CHROMIUM=$(command -v chromium 2>/dev/null || command -v chromium-browser 2>/dev/null) \
    && ln -sf "$CHROMIUM" /usr/local/bin/chromium-puppeteer
ENV PUPPETEER_EXECUTABLE_PATH=/usr/local/bin/chromium-puppeteer

# Select the correct base stage based on target architecture
ARG TARGETARCH
FROM base-${TARGETARCH} AS base

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
RUN dotnet restore "MangaShelf.Parser/MangaShelf.Parser.csproj"

# Copy the rest of the source code
COPY . .

WORKDIR "/src/MangaShelf.Parser"
RUN dotnet build "./MangaShelf.Parser.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./MangaShelf.Parser.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MangaShelf.Parser.dll"]