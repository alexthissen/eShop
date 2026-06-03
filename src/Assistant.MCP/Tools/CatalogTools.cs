using System.ComponentModel;
using System.Net.Http.Json;
using eShop.Assistant.Mcp.Models;
using ModelContextProtocol.Server;

namespace eShop.Assistant.Mcp.Tools;

[McpServerToolType]
public sealed class CatalogTools(IHttpClientFactory httpClientFactory)
{
    private HttpClient CatalogClient => httpClientFactory.CreateClient("CatalogApi");

    [McpServerTool(Name = "search_catalog"),
     Description("Search the product catalog using a natural language description. Returns products semantically relevant to the query.")]
    public async Task<string> SearchCatalog(
        [Description("Natural language product description, e.g. 'blue running shoes' or 'warm winter jacket'")] string query,
        [Description("Number of results to return (default 5, max 20)")] int maxResults = 5)
    {
        maxResults = Math.Clamp(maxResults, 1, 20);
        var result = await CatalogClient.GetFromJsonAsync<PaginatedItems<CatalogItem>>(
            $"api/catalog/items/withsemanticrelevance/{Uri.EscapeDataString(query)}?pageSize={maxResults}&pageIndex=0");

        if (result is null || result.Data.Length == 0)
            return "No products found matching your description.";

        return string.Join("\n\n", result.Data.Select(FormatItem));
    }

    [McpServerTool(Name = "get_catalog_items"),
     Description("Browse products in the catalog with optional filtering by brand or product type.")]
    public async Task<string> GetCatalogItems(
        [Description("Page number starting from 0")] int page = 0,
        [Description("Number of items per page (default 10, max 20)")] int pageSize = 10,
        [Description("Optional brand ID to filter by")] int? brandId = null,
        [Description("Optional product type ID to filter by")] int? typeId = null)
    {
        pageSize = Math.Clamp(pageSize, 1, 20);
        var url = $"api/catalog/items?pageIndex={page}&pageSize={pageSize}&api-version=1.0";
        if (typeId.HasValue && brandId.HasValue)
            url = $"api/catalog/items/type/{typeId}/brand/{brandId}?pageIndex={page}&pageSize={pageSize}&api-version=1.0";
        else if (brandId.HasValue)
            url = $"api/catalog/items/type/all/brand/{brandId}?pageIndex={page}&pageSize={pageSize}&api-version=1.0";

        var result = await CatalogClient.GetFromJsonAsync<PaginatedItems<CatalogItem>>(url);
        if (result is null || result.Data.Length == 0)
            return "No products found.";

        var header = $"Showing {result.Data.Length} of {result.Count} products (page {result.PageIndex + 1}):";
        return header + "\n\n" + string.Join("\n\n", result.Data.Select(FormatItem));
    }

    [McpServerTool(Name = "get_product_details"),
     Description("Get detailed information about a specific product by its ID.")]
    public async Task<string> GetProductDetails(
        [Description("The product ID")] int productId)
    {
        var response = await CatalogClient.GetAsync($"api/catalog/items/{productId}");
        if (!response.IsSuccessStatusCode)
            return $"Product with ID {productId} not found.";

        var item = await response.Content.ReadFromJsonAsync<CatalogItem>();
        return item is null ? "Product not found." : FormatItem(item);
    }

    [McpServerTool(Name = "get_brands"),
     Description("List all available product brands.")]
    public async Task<string> GetBrands()
    {
        var brands = await CatalogClient.GetFromJsonAsync<CatalogBrand[]>("api/catalog/catalogbrands?api-version=1.0");
        if (brands is null || brands.Length == 0)
            return "No brands available.";

        return "Available brands:\n" + string.Join("\n", brands.Select(b => $"- [{b.Id}] {b.Brand}"));
    }

    [McpServerTool(Name = "get_product_types"),
     Description("List all available product types/categories.")]
    public async Task<string> GetProductTypes()
    {
        var types = await CatalogClient.GetFromJsonAsync<CatalogType[]>("api/catalog/catalogtypes?api-version=1.0");
        if (types is null || types.Length == 0)
            return "No product types available.";

        return "Available product types:\n" + string.Join("\n", types.Select(t => $"- [{t.Id}] {t.Type}"));
    }

    private static string FormatItem(CatalogItem item) =>
        $"**{item.Name}** (ID: {item.Id})\n" +
        $"  Price: ${item.Price:F2}\n" +
        $"  Brand: {item.CatalogBrand?.Brand ?? "N/A"} | Type: {item.CatalogType?.Type ?? "N/A"}\n" +
        $"  In stock: {item.AvailableStock}\n" +
        (item.Description is not null ? $"  Description: {item.Description}" : "");
}
