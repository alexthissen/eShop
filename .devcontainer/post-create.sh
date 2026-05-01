#!/bin/sh

sudo dotnet workload update 
sudo dotnet workload restore

dotnet tool restore

sudo apt-get update && sudo apt-get install -y protobuf-compiler

# Import and trust the ASP.NET Core HTTPS development certificate
dotnet dev-certs https --import /https/eshop-ssl.pfx --clean --password $PFX_PASSWORD --verbose
dotnet dev-certs https --trust --verbose
dotnet dev-certs https --check --trust --verbose

devproxy cert ensure
sudo openssl pkcs12 -in $HOME/.config/dev-proxy/rootCert.pfx -nokeys -nodes \
  -out /usr/local/share/ca-certificates/devproxy-ca.crt -passin pass:
sudo update-ca-certificates