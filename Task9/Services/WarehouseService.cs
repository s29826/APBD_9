using Microsoft.Data.SqlClient;

namespace Task9.Services;

public class WarehouseService : IWarehouseService
{
    private readonly string _connectionString = "Data Source=db-mssql;Initial Catalog=2019SBD;Integrated Security=True;Trust Server Certificate=True";
    public async Task<bool> DoesProductExist(int idProduct)
    {
        string sql = "SELECT COUNT(*)\n" +
                     "FROM Product\n" +
                     "WHERE IdProduct = @idProduct";
        
        await using SqlConnection conn = new SqlConnection(_connectionString);
        await using SqlCommand cmd = new SqlCommand(sql, conn);

        await conn.OpenAsync();

        cmd.Parameters.AddWithValue("@idProduct", idProduct);

        var result = await cmd.ExecuteScalarAsync();
        
        
        return Convert.ToInt32(result) > 0;
    }

    public async Task<bool> DoesWarehouseExist(int idWarehouse)
    {
        string sql = "SELECT COUNT(*)\n" +
                     "FROM Warehouse\n" +
                     "WHERE IdWarehouse = @idWarehouse";
        
        await using SqlConnection conn = new SqlConnection(_connectionString);
        await using SqlCommand cmd = new SqlCommand(sql, conn);

        await conn.OpenAsync();

        cmd.Parameters.AddWithValue("@idWarehouse", idWarehouse);

        var result = await cmd.ExecuteScalarAsync();
        
        
        return Convert.ToInt32(result) > 0;
    }

    public async Task<bool> CanAddToOrder(int idProduct, int amount)
    {
        string sql = "SELECT COUNT(*)\n" +
                     "FROM [Order]\n" +
                     "WHERE IdProduct = @idProduct AND Amount >= @amount";
        
        await using SqlConnection conn = new SqlConnection(_connectionString);
        await using SqlCommand cmd = new SqlCommand(sql, conn);

        await conn.OpenAsync();

        cmd.Parameters.AddWithValue("@idWarehouse", idProduct);
        cmd.Parameters.AddWithValue("@amount", amount);

        var result = await cmd.ExecuteScalarAsync();
        
        
        return Convert.ToInt32(result) > 0;
    }

    public async Task<bool> IsItDone(int idOrder)
    {
        string sql = "SELECT COUNT(*)\n" +
                     "FROM Product_Warehouse\n" +
                     "WHERE IdOrder = @idOrder";
        
        await using SqlConnection conn = new SqlConnection(_connectionString);
        await using SqlCommand cmd = new SqlCommand(sql, conn);

        await conn.OpenAsync();

        cmd.Parameters.AddWithValue("@idOrder", idOrder);

        var result = await cmd.ExecuteScalarAsync();
        
        
        return Convert.ToInt32(result) > 0;
    }
}