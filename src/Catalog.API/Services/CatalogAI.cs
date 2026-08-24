using System.Diagnostics;
using Microsoft.Extensions.AI;
using Pgvector;
using Microsoft.FeatureManagement;

namespace eShop.Catalog.API.Services;

public sealed class CatalogAI : ICatalogAI
{
    private const int EmbeddingDimensions = 384;
    private readonly IEmbeddingGenerator<string, Embedding<float>>? _embeddingGenerator;

    /// <summary>The web host environment.</summary>
    private readonly IWebHostEnvironment _environment;
    /// <summary>Logger for use in AI operations.</summary>
    private readonly ILogger<CatalogAI> _logger;
    private readonly IFeatureManager _featureManager;

    public CatalogAI(IWebHostEnvironment environment, 
        ILogger<CatalogAI> logger, 
        IFeatureManager featureManager,
        IEmbeddingGenerator<string, Embedding<float>>? embeddingGenerator = null)
    {
        _featureManager = featureManager;
        _embeddingGenerator = embeddingGenerator;
        _environment = environment;
        _logger = logger;

        IsEnabled = 
            _featureManager.IsEnabledAsync("CatalogAI").GetAwaiter().GetResult()
            && _embeddingGenerator is not null;
    }

    /// <inheritdoc/>
    public bool IsEnabled { get; }

    /// <inheritdoc/>
    public ValueTask<Vector?> GetEmbeddingAsync(CatalogItem item) =>
        IsEnabled ?
            GetEmbeddingAsync(CatalogItemToString(item)) :
            ValueTask.FromResult<Vector?>(null);

    /// <inheritdoc/>
    public async ValueTask<IReadOnlyList<Vector>?> GetEmbeddingsAsync(IEnumerable<CatalogItem> items)
    {
        if (IsEnabled)
        {
            long timestamp = Stopwatch.GetTimestamp();

            GeneratedEmbeddings<Embedding<float>> embeddings = await _embeddingGenerator!.GenerateAsync(items.Select(CatalogItemToString));
            var results = embeddings.Select(m => new Vector(m.Vector[0..EmbeddingDimensions])).ToList();

            if (_logger.IsEnabled(LogLevel.Trace))
            {
                _logger.LogTrace("Generated {EmbeddingsCount} embeddings in {ElapsedMilliseconds}s", results.Count, Stopwatch.GetElapsedTime(timestamp).TotalSeconds);
            }

            return results;
        }

        return null;
    }

    /// <inheritdoc/>
    public async ValueTask<Vector?> GetEmbeddingAsync(string text)
    {
        if (IsEnabled)
        {
            long timestamp = Stopwatch.GetTimestamp();

            var embedding = await _embeddingGenerator!.GenerateVectorAsync(text);
            embedding = embedding[0..EmbeddingDimensions];

            if (_logger.IsEnabled(LogLevel.Trace))
            {
                _logger.LogTrace("Generated embedding in {ElapsedMilliseconds}s: '{Text}'", Stopwatch.GetElapsedTime(timestamp).TotalSeconds, text);
            }

            return new Vector(embedding);
        }

        return null;
    }

    private static string CatalogItemToString(CatalogItem item) => $"{item.Name} {item.Description}";
}
