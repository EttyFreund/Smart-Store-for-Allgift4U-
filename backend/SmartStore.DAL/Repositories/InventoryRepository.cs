using System.Data;
using Microsoft.Data.SqlClient;

namespace SmartStore.DAL.Repositories;

public class InventoryRepository
{
    private readonly string _connectionString;

    public InventoryRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public DataTable GetOrderComponentsByName(string orderName)
    {
        var table = new DataTable();
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand("sp_GetOrderComponentsByName", connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        command.Parameters.AddWithValue("@OrderName", orderName);
        connection.Open();
        using var reader = command.ExecuteReader();
        table.Load(reader);
        return table;
    }

    public void UpdateProductQuantity(int productId, int quantityToRemove)
    {
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(
            "UPDATE Products SET CurrentQuantity = CurrentQuantity - @quantityToRemove WHERE ProductID = @productId",
            connection);
        command.Parameters.AddWithValue("@quantityToRemove", quantityToRemove);
        command.Parameters.AddWithValue("@productId", productId);
        connection.Open();
        command.ExecuteNonQuery();
    }

    public void InsertRecommendation(int productId, string supplierName, decimal price, string purchaseUrl)
    {
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand("sp_InsertRecommendation", connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        command.Parameters.AddWithValue("@ProductID", productId);
        command.Parameters.AddWithValue("@SupplierName", supplierName);
        command.Parameters.AddWithValue("@Price", price);
        command.Parameters.AddWithValue("@PurchaseUrl", purchaseUrl);
        connection.Open();
        command.ExecuteNonQuery();
    }
}
