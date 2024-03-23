# https://hub.docker.com/_/microsoft-dotnet
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /source

# Copy fsproj and restore all dependencies
COPY ./*.fsproj ./
RUN dotnet restore

# Copy source code and build / publish app and libraries
COPY . .
RUN dotnet publish -c release -o /app --no-restore

# Run project
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS final
WORKDIR /app
COPY --from=build /app .

# Run as a non-root user
RUN groupadd -r appgroup && useradd -r -g appgroup appuser
USER appuser

# Health check example (adjust as needed)
HEALTHCHECK --interval=30s --timeout=3s --retries=3 \
    CMD curl --fail http://localhost:80/healthz || exit 1

# Expose the port the app runs on
EXPOSE 80

# Specify the entry point
ENTRYPOINT ["dotnet", "YOUR_APP_NAME.dll"]
