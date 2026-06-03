using eShop.Assistant.Mcp.Tools;
using eShop.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddHttpClient("CatalogApi", client =>
{
    client.BaseAddress = new("https+http://catalog-api");
});

builder.Services.AddHttpClient("OrderingApi", client =>
{
    client.BaseAddress = new("https+http://ordering-api");
});

builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithTools<CatalogTools>();

var app = builder.Build();

app.MapDefaultEndpoints();
app.MapMcp();

app.Run();
