# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base

ARG TARGETARCH
RUN if [ "$TARGETARCH" != "arm64" ] ; then \
	  ##Install dependencies for PuppeteerSharp (Chromium)
	  ## 25.02.2026 works on AMD64, but not on ARM64. .NET 10.0.3
	  apt-get update && apt-get install curl gnupg -y \
	  && curl --location --silent https://dl-ssl.google.com/linux/linux_signing_key.pub | apt-key add - \
	  && sh -c 'echo "deb [arch=amd64] http://dl.google.com/linux/chrome/deb/ stable main" >> /etc/apt/sources.list.d/google.list' \
	  && apt-get update \
	  && apt-get install google-chrome-stable -y --no-install-recommends \
	  && rm -rf /var/lib/apt/lists/*; \
	  export PUPPETEER_EXECUTABLE_PATH="/usr/bin/google-chrome"; \
    else \
	  ## 25.02.2026 works on ARM64, but only on .NET 8
	  apt-get update && apt-get install -y chromium --no-install-recommends --allow-downgrades fonts-ipafont-gothic fonts-wqy-zenhei fonts-thai-tlwg fonts-kacst fonts-freefont-ttf; \
	  export PUPPETEER_EXECUTABLE_PATH="/usr/bin/chromium"; \
	fi

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