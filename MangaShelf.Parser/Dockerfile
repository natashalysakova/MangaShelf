# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app

##Install dependencies for PuppeteerSharp (Chromium)
#RUN apt-get update \
    #&& apt-get install -y wget gnupg \
    #&& wget -q -O - https://dl-ssl.google.com/linux/linux_signing_key.pub | apt-key add - \
    #&& sh -c 'echo "deb [arch=amd64] http://dl.google.com/linux/chrome/deb/ stable main" >> /etc/apt/sources.list.d/google.list' \
    #&& apt-get update \
    #&& apt-get install -y google-chrome-stable fonts-ipafont-gothic fonts-wqy-zenhei fonts-thai-tlwg fonts-kacst fonts-freefont-ttf libxss1 \
      #--no-install-recommends \
    #&& rm -rf /var/lib/apt/lists/*
#
######################
##PUPPETEER RECIPE based on https://github.com/hardkoded/puppeteer-sharp/issues/1180#issuecomment-1015532968
######################
#RUN apt-get update && apt-get -f install && apt-get -y install wget gnupg2 apt-utils
#RUN wget -q -O - https://dl.google.com/linux/linux_signing_key.pub | apt-key add -
#RUN echo 'deb [arch=amd64] http://dl.google.com/linux/chrome/deb/ stable main' >> /etc/apt/sources.list
#RUN apt-get update \
#&& apt-get install -y google-chrome-stable --no-install-recommends --allow-downgrades fonts-ipafont-gothic fonts-wqy-zenhei fonts-thai-tlwg fonts-kacst fonts-freefont-ttf
#######################
##END PUPPETEER RECIPE
#######################
#ENV PUPPETEER_EXECUTABLE_PATH "/usr/bin/google-chrome-stable"

#####################
#PUPPETEER RECIPE 
#####################
RUN apt-get update && apt-get install -y chromium --no-install-recommends --allow-downgrades fonts-ipafont-gothic fonts-wqy-zenhei fonts-thai-tlwg fonts-kacst fonts-freefont-ttf
######################
#END PUPPETEER RECIPE
######################
ENV PUPPETEER_EXECUTABLE_PATH "/usr/bin/chromium"


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy all csproj files first to leverage Docker layer caching for restore
COPY ["MangaShelf.Parser/MangaShelf.Parser.csproj", "MangaShelf.Parser/"]
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