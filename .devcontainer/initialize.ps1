# Set password to environment variable or fallback to default
$password = if ([string]::IsNullOrEmpty($args[0])) { "DefaultPassword!" } else { $args[0] }

# Export ASP.NET Core development certificate to PFX file in mount directory
dotnet dev-certs https --export-path $HOME/.aspnet/https/eshop-ssl.pfx --password $password --verbose