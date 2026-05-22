# Use .NET 10.0 as the stable base image
FROM mcr.microsoft.com/devcontainers/dotnet:dev-10.0 AS base

# Copy .NET 11.0 SDK from the sdk image
COPY --from=mcr.microsoft.com/dotnet/sdk:11.0.100-preview.4 /usr/share/dotnet /usr/share/dotnet
