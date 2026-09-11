using eShop.Catalog.API.Services;
using Microsoft.FeatureManagement;
using OpenTelemetry.Trace;

public static class Extensions
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        // Avoid loading full database config and migrations if startup
        // is being invoked from build-time OpenAPI generation
        if (builder.Environment.IsBuild())
        {
            builder.Services.AddDbContext<CatalogContext>();
            builder.Services.AddFeatureManagement();
            builder.Services.Configure<FeatureManagementOptions>(options =>
            {
                options.IgnoreMissingFeatureFilters = true;
            });
            return;
        }

        builder.AddNpgsqlDbContext<CatalogContext>("catalogdb", configureDbContextOptions: dbContextOptionsBuilder =>
        {
            dbContextOptionsBuilder.UseNpgsql(builder =>
            {
                builder.UseVector();
            });
        });

        // REVIEW: This is done for development ease but shouldn't be here in production
        builder.Services.AddMigration<CatalogContext, CatalogContextSeed>();

        // Add the integration services that consume the DbContext
        builder.Services.AddTransient<IIntegrationEventLogService, IntegrationEventLogService<CatalogContext>>();

        builder.Services.AddTransient<ICatalogIntegrationEventService, CatalogIntegrationEventService>();

        builder.AddRabbitMqEventBus("eventbus")
               .AddSubscription<OrderStatusChangedToAwaitingValidationIntegrationEvent, OrderStatusChangedToAwaitingValidationIntegrationEventHandler>()
               .AddSubscription<OrderStatusChangedToPaidIntegrationEvent, OrderStatusChangedToPaidIntegrationEventHandler>();

        builder.Services.AddOptions<CatalogOptions>()
            .BindConfiguration(nameof(CatalogOptions));

//        builder.Configuration["ConnectionStrings:appconfig"] = "<your-connection-string-here>";

        // Configure feature management FIRST to ensure IgnoreMissingFeatureFilters is set
        // before any feature flags are evaluated during configuration loading
        builder.Services.Configure<FeatureManagementOptions>(options =>
        {
            options.IgnoreMissingFeatureFilters = true;
        });

        builder.AddAzureAppConfiguration(
            "appconfig",
            configureOptions: options =>
            {
                // Select only keys that start with "CatalogAPI:" for regular configuration
                // This removes the default "*" query that would load all keys including all feature flags
                options.Select("CatalogAPI:*");
                options.UseFeatureFlags(config => config.Select("CatalogAPI:*"));

                // Configure refresh
                options.ConfigureRefresh(refresh =>
                {
                    refresh.Register("eShop:Sentinel", refreshAll: true)
                        .SetRefreshInterval(TimeSpan.FromSeconds(30));
                });
            });

        builder.Services.AddFeatureManagement();

        builder.Services.ConfigureOpenTelemetryTracerProvider(tracing =>
            tracing.AddSource("Microsoft.FeatureManagement"));

        if (builder.Configuration["OllamaEnabled"] is string ollamaEnabled && bool.Parse(ollamaEnabled))
        {
            builder.AddOllamaApiClient("embedding")
                .AddEmbeddingGenerator();
        }
        else if (!string.IsNullOrWhiteSpace(builder.Configuration.GetConnectionString("textEmbeddingModel")))
        {
            builder.AddOpenAIClientFromConfiguration("textEmbeddingModel")
                .AddEmbeddingGenerator();
        }

        builder.Services.AddScoped<ICatalogAI, CatalogAI>();
    }
}
