# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
RUN dotnet dev-certs https
RUN dotnet dev-certs https --trust
WORKDIR /src
COPY ["MangaShelf/MangaShelf.csproj", "MangaShelf/"]
RUN dotnet restore "./MangaShelf/MangaShelf.csproj"
COPY . .
WORKDIR "/src/MangaShelf"
RUN dotnet build "./MangaShelf.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./MangaShelf.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Before the final CMD or ENTRYPOINT
# Switch to the root user to change permissions
USER root

# Create the directory and change its ownership to the 'app' user.
# The -R flag ensures that if the directory already exists, its contents also get the correct ownership.
RUN mkdir -p /app/wwwroot/images && chown -R app:app /app/wwwroot/images

# Switch back to the non-root user to run the application
USER app

ENTRYPOINT ["dotnet", "MangaShelf.dll"]