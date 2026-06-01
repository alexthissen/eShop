using System.ComponentModel;
using System.Net.Http.Json;
using eShop.McpServer.Models;
using ModelContextProtocol.Server;

namespace eShop.McpServer.Tools;

[McpServerToolType]
public sealed class OrderTools(IHttpClientFactory httpClientFactory)
{
    private HttpClient OrderingClient => httpClientFactory.CreateClient("OrderingApi");

    [McpServerTool(Name = "list_orders"),
     Description("List the current user's orders with their status and total amount.")]
    public async Task<string> ListOrders()
    {
        var orders = await OrderingClient.GetFromJsonAsync<OrderSummary[]>("api/orders");
        if (orders is null || orders.Length == 0)
            return "No orders found.";

        return "Your orders:\n\n" + string.Join("\n", orders.Select(o =>
            $"- Order #{o.OrderNumber} | {o.Date:yyyy-MM-dd} | Status: {o.Status} | Total: ${o.Total:F2}"));
    }

    [McpServerTool(Name = "get_order_details"),
     Description("Get full details of a specific order by its order number.")]
    public async Task<string> GetOrderDetails(
        [Description("The order number")] int orderNumber)
    {
        var response = await OrderingClient.GetAsync($"api/orders/{orderNumber}");
        if (!response.IsSuccessStatusCode)
            return $"Order #{orderNumber} not found.";

        var json = await response.Content.ReadAsStringAsync();
        return $"Order #{orderNumber} details:\n{json}";
    }
}
