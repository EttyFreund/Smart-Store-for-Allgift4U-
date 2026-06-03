using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using SmartStore.BLL.Interfaces;
using SmartStore.DAL.Repositories;

namespace SmartStore.BLL.Services;

public class InventoryService : IInventoryService
{
    private readonly InventoryRepository _repository;
    private readonly string _groqApiKey;
    private static readonly HttpClient _httpClient = new();

    public InventoryService(InventoryRepository repository, string groqApiKey)
    {
        _repository = repository;
        _groqApiKey = groqApiKey;
    }

    public async Task<OrderResult> ProcessIncomingOrder(string orderName)
    {
        var table = _repository.GetOrderComponentsByName(orderName);
        if (table.Rows.Count == 0)
            throw new Exception($"ההזמנה '{orderName}' לא נמצאה או שאין לה מוצרים.");

        var result = new OrderResult { OrderName = orderName };

        foreach (System.Data.DataRow row in table.Rows)
        {
            int productId = Convert.ToInt32(row["ProductID"]);
            string productName = row["ProductName"].ToString()!;
            int quantityRequired = Convert.ToInt32(row["QuantityRequired"]);
            int currentQuantity = Convert.ToInt32(row["CurrentQuantity"]);

            _repository.UpdateProductQuantity(productId, quantityRequired);
            int newQuantity = currentQuantity - quantityRequired;
            bool aiTriggered = newQuantity <= 0;

            if (aiTriggered)
            {
                var aiResult = await RunAiPurchasingAgent(productName);
                await ParseAndSaveRecommendation(productId, aiResult);
            }

            result.Items.Add(new OrderItemResult
            {
                ProductName = productName,
                QuantityRemoved = quantityRequired,
                NewQuantity = newQuantity,
                AiTriggered = aiTriggered
            });
        }

        return result;
    }

    public async Task<string> RunAiPurchasingAgent(string productName)
    {
        var requestBody = new
        {
            model = "llama-3.3-70b-versatile",
            messages = new[]
            {
                new
                {
                    role = "user",
                    content = $"You are a procurement assistant. Find a real, well-known Israeli online retailer that sells '{productName}'. " +
                              $"Use only real websites that actually exist, such as: ksp.co.il, ivory.co.il, bug.co.il, zap.co.il, amazon.com, aliexpress.com, ace.co.il, officedepot.co.il. " +
                              $"Return ONLY a valid JSON object with these exact fields: " +
                              $"SupplierName (string - the store name), " +
                              $"Price (number - estimated price in ILS), " +
                              $"PurchaseUrl (string - the homepage URL of the store, e.g. https://www.ksp.co.il). " +
                              $"Do NOT invent URLs. Use only the homepage of a real known store."
                }
            },
            response_format = new { type = "json_object" }
        };

        var json = JsonSerializer.Serialize(requestBody);
        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.groq.com/openai/v1/chat/completions")
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _groqApiKey);

        var response = await _httpClient.SendAsync(request);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Groq API error: {responseBody}");

        using var doc = JsonDocument.Parse(responseBody);
        return doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString()!;
    }

    private async Task ParseAndSaveRecommendation(int productId, string jsonResult)
    {
        using var doc = JsonDocument.Parse(jsonResult);
        var root = doc.RootElement;

        string supplierName = root.GetProperty("SupplierName").GetString() ?? "Unknown";
        decimal price = root.GetProperty("Price").GetDecimal();
        string purchaseUrl = root.GetProperty("PurchaseUrl").GetString() ?? "";

        await Task.Run(() => _repository.InsertRecommendation(productId, supplierName, price, purchaseUrl));
    }
}
