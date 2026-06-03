namespace SmartStore.BLL.Interfaces;

public interface IInventoryService
{
    Task<OrderResult> ProcessIncomingOrder(string orderName);
}

public class OrderResult
{
    public string OrderName { get; set; } = string.Empty;
    public List<OrderItemResult> Items { get; set; } = new();
}

public class OrderItemResult
{
    public string ProductName { get; set; } = string.Empty;
    public int QuantityRemoved { get; set; }
    public int NewQuantity { get; set; }
    public bool AiTriggered { get; set; }
}
