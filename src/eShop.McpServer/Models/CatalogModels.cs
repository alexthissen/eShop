namespace eShop.McpServer.Models;

public record CatalogItem(
    int Id,
    string Name,
    string? Description,
    decimal Price,
    string? PictureFileName,
    int CatalogTypeId,
    CatalogBrand? CatalogBrand,
    int CatalogBrandId,
    CatalogType? CatalogType,
    int AvailableStock);

public record CatalogBrand(int Id, string Brand);

public record CatalogType(int Id, string Type);

public record PaginatedItems<T>(int PageIndex, int PageSize, long Count, T[] Data);
