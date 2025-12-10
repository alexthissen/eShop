# Use .NET 8.0 as the stable base image
FROM mcr.microsoft.com/devcontainers/dotnet:1-8.0-bookworm AS base

# Copy .NET 10.0 SDK from the sdk image
COPY --from=mcr.microsoft.com/dotnet/sdk:10.0 /usr/share/dotnet /usr/share/dotnet
