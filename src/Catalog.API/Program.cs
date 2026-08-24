using Microsoft.FeatureManagement;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddApplicationServices();
builder.Services.AddProblemDetails();

var withApiVersioning = builder.Services.AddApiVersioning(options =>
{
    // Include "api-supported-versions" and "api-deprecated-versions" headers in all responses
    options.ReportApiVersions = true;
});

builder.AddDefaultOpenApi(withApiVersioning);

var app = builder.Build();

var manager = app.Services.GetRequiredService<IFeatureManager>();
await foreach (var name in manager.GetFeatureNamesAsync())
{
    var enabled = await manager.IsEnabledAsync(name);
    Console.WriteLine($"Feature {name} is {(enabled ? "enabled" : "disabled")}");
}

Console.WriteLine(((IConfigurationRoot)builder.Configuration).GetDebugView());

app.MapDefaultEndpoints();
app.UseStatusCodePages();
app.MapCatalogApi();
app.UseDefaultOpenApi();

app.Run();
