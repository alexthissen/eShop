namespace eShop.McpServer.Models;

public record OrderSummary(int OrderNumber, DateTime Date, string Status, double Total);
