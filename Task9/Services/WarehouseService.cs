using System.Data;
using Microsoft.Data.SqlClient;
using Task9.Models.DTOs;

namespace Task9.Services;

public class WarehouseService : IWarehouseService
{
    private readonly IConfiguration _configuration;
    public WarehouseService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<int> AddProduct(ProductDTO productDto)
    {
        if (productDto.Amount <= 0)
        {
            return -5;
        }

        await using var conn = new SqlConnection(_configuration.GetConnectionString("Default"));
        await conn.OpenAsync();
        await using var tran = (SqlTransaction) await conn.BeginTransactionAsync();

        try
        {
            if (!await DoesProductExist(productDto.IdProduct, conn, tran))
                return -1;

            if (!await DoesWarehouseExist(productDto.IdWarehouse, conn, tran))
                return -2;

            if (!await CanAddToOrder(productDto.IdProduct, productDto.Amount, productDto.CreatedAt, conn, tran))
                return -3;

            int idOrder = await FindIdOrder(productDto, conn, tran);

            if (await AlreadyDone(idOrder, conn, tran))
                return -4;

            await UpdateOrderFullilledAt(idOrder, conn, tran);

            
            Decimal price = await GetProductPrice(productDto.IdProduct, conn, tran) * productDto.Amount;

            string sql = "INSERT INTO Product_Warehouse (IdWarehouse, IdProduct, IdOrder, Amount, Price, CreatedAt)\n" +
                         "OUTPUT inserted.IdProductWarehouse\n" +
                         "VALUES (@idWarehouse, @idProduct, @idOrder, @amount, @price, getdate())";

            await using (var cmd = new SqlCommand(sql, conn, tran))
            {
                cmd.Parameters.AddWithValue("@idWarehouse", productDto.IdWarehouse);
                cmd.Parameters.AddWithValue("@idProduct", productDto.IdProduct);
                cmd.Parameters.AddWithValue("@idOrder", idOrder);
                cmd.Parameters.AddWithValue("@amount", productDto.Amount);
                cmd.Parameters.AddWithValue("@price", price);

                var newId = await cmd.ExecuteScalarAsync();
                await tran.CommitAsync();
                
                
                return Convert.ToInt32(newId);
            }
        }
        catch (SqlException)
        {
            await tran.RollbackAsync();
            return -5;

        }
    }

    public async Task<int> AddProductProcedure(ProductDTO productDto)
    {
        await using SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand cmd = new SqlCommand();

        cmd.Connection = conn;
        cmd.CommandText = "AddProductToWarehouse";
        cmd.CommandType = CommandType.StoredProcedure;

        await conn.OpenAsync();

        cmd.Parameters.AddWithValue("IdProduct", productDto.IdProduct);
        cmd.Parameters.AddWithValue("@IdWarehouse", productDto.IdWarehouse);
        cmd.Parameters.AddWithValue("@Amount", productDto.Amount);
        cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now);

        var output = await cmd.ExecuteScalarAsync();


        return Convert.ToInt32(output);
    }

    private async Task<bool> DoesProductExist(int idProduct, SqlConnection sqlConnection, SqlTransaction sqlTransaction)
    {
        string sql = "SELECT COUNT(*)\n" +
                     "FROM Product\n" +
                     "WHERE IdProduct = @idProduct";
        
        await using SqlCommand cmd = new SqlCommand(sql, sqlConnection, sqlTransaction);

        cmd.Parameters.AddWithValue("@idProduct", idProduct);

        var result = await cmd.ExecuteScalarAsync();
        
        
        return Convert.ToInt32(result) > 0;
    }

    private async Task<bool> DoesWarehouseExist(int idWarehouse, SqlConnection sqlConnection, SqlTransaction sqlTransaction)
    {
        string sql = "SELECT COUNT(*)\n" +
                     "FROM Warehouse\n" +
                     "WHERE IdWarehouse = @idWarehouse";
        
        await using SqlCommand cmd = new SqlCommand(sql, sqlConnection, sqlTransaction);

        cmd.Parameters.AddWithValue("@idWarehouse", idWarehouse);

        var result = await cmd.ExecuteScalarAsync();
        
        
        return Convert.ToInt32(result) > 0;
    }

    private async Task<bool> CanAddToOrder(int idProduct, int amount, DateTime dateTime, SqlConnection sqlConnection, SqlTransaction sqlTransaction)
    {
        string sql = "SELECT COUNT(*)\n" +
                     "FROM [Order]\n" +
                     "WHERE IdProduct = @idProduct AND Amount >= @amount AND CreatedAt < @createdAt";
        
        await using SqlCommand cmd = new SqlCommand(sql, sqlConnection, sqlTransaction);

        cmd.Parameters.AddWithValue("@idProduct", idProduct);
        cmd.Parameters.AddWithValue("@amount", amount);
        cmd.Parameters.AddWithValue("@createdAt", dateTime);

        var result = await cmd.ExecuteScalarAsync();
        
        
        return Convert.ToInt32(result) > 0;
    }

    private async Task<bool> AlreadyDone(int idOrder, SqlConnection sqlConnection, SqlTransaction sqlTransaction)
    {
        string sql = "SELECT COUNT(*)\n" +
                     "FROM Product_Warehouse\n" +
                     "WHERE IdOrder = @idOrder";
        
        await using SqlCommand cmd = new SqlCommand(sql, sqlConnection, sqlTransaction);

        cmd.Parameters.AddWithValue("@idOrder", idOrder);

        var result = await cmd.ExecuteScalarAsync();
        
        
        return Convert.ToInt32(result) > 0;
    }

    private async Task<int> FindIdOrder(ProductDTO productDto, SqlConnection sqlConnection, SqlTransaction sqlTransaction)
    {
        string sql = "SELECT IdOrder\n" +
                     "FROM [Order]\n" +
                     "WHERE IdProduct = @IdProduct AND Amount = @amount";
        
        await using SqlCommand cmd = new SqlCommand(sql, sqlConnection, sqlTransaction);

        cmd.Parameters.AddWithValue("@idProduct", productDto.IdProduct);
        cmd.Parameters.AddWithValue("@amount", productDto.Amount);


        return Convert.ToInt32(await cmd.ExecuteScalarAsync());
    }

    private async Task UpdateOrderFullilledAt(int idOrder, SqlConnection sqlConnection, SqlTransaction sqlTransaction)
    {
        string sql = "UPDATE [Order]\n" +
                     "SET FulfilledAt = getdate()\n" +
                     "WHERE IdOrder = @idOrder";
        
        await using SqlCommand cmd = new SqlCommand(sql, sqlConnection, sqlTransaction);

        cmd.Parameters.AddWithValue("@idOrder", idOrder);

        
        await cmd.ExecuteNonQueryAsync();
    }

    private async Task<Decimal> GetProductPrice(int idProduct, SqlConnection sqlConnection, SqlTransaction sqlTransaction)
    {
        string sql = "SELECT Price\n" +
                     "FROM Product\n" +
                     "WHERE IdProduct = @idProduct";
        
        await using SqlCommand cmd = new SqlCommand(sql, sqlConnection, sqlTransaction);

        cmd.Parameters.AddWithValue("@idProduct", idProduct);


        return Convert.ToDecimal(await cmd.ExecuteScalarAsync());
    }
}