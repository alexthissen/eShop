#!/bin/sh

# Change ownership of the .dotnet directory to the vscode user (to avoid permission errors)
sudo chown -R vscode:vscode /home/vscode/.dotnet
    
# Restore .NET workloads
sudo dotnet workload update

# Install Agent Package Manager CLI tool
curl -sSL https://aka.ms/apm-unix | sh
apm --version