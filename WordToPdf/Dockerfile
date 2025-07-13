# Stage 1: Base runtime image with LibreOffice installed
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base

# Install LibreOffice and required dependencies
RUN apt-get update && \
    apt-get install -y libreoffice && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

# Optional: Remove or define APP_UID to avoid errors
# USER $APP_UID  # <- REMOVE or define it via ARG/ENV if needed

WORKDIR /app

# Expose application ports
EXPOSE 8080
EXPOSE 8081

# Stage 2: Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy only the project file and restore dependencies
COPY ["WordToPdf/WordToPdf.csproj", "WordToPdf/"]
RUN dotnet restore "WordToPdf/WordToPdf.csproj"

# Copy the remaining files and build
COPY . .
WORKDIR "/src/WordToPdf"
RUN dotnet build "WordToPdf.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Stage 3: Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
WORKDIR /src/WordToPdf
RUN dotnet publish "WordToPdf.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Stage 4: Final runtime image
FROM base AS final
WORKDIR /app

# Copy published output
COPY --from=publish /app/publish .

# Set entrypoint
ENTRYPOINT ["dotnet", "WordToPdf.dll"]