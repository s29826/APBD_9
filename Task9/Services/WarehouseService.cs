using System.Data;
using Microsoft.Data.SqlClient;
using Task9.Models.DTOs;

namespace Task9.Services;

public class WarehouseService : IWarehouseService
{
    private readonly string _connectionString = "Data Source=db-mssql;Initial Catalog=2019SBD;Integrated Security=True;Trust Server Certificate=True";

    public async Task<int> AddProduct(ProductDTO productDto)
    {
        if (productDto.Amount <= 0)
        {
            return -5;
        }

        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var tran = (SqlTransaction) await conn.BeginTransactionAsync();

        try
        {
            if (!await DoesProductExist(productDto.IdProduct, conn, tran))
                return -1;

            if (!await DoesWarehouseExist(productDto.IdWarehouse, conn, tran))
                return -2;

            if (!await CanAddToOrder(productDto.IdProduct, productDto.Amount, conn, tran))
                return -3;

            int idOrder = await FindIdOrder(productDto, conn, tran);

            if (await AlreadyDone(idOrder, conn, tran))
                return -4;

            await UpdateOrderFullilledAt(idOrder, conn, tran);

            
            Decimal price = await GetProductPrice(productDto.IdProduct, conn, tran);

            string sql = "INSERT INTO Product_Warehouse (IdWarehouse, IdProduct, IdOrder, Amount, Price, CreatedAt)\n" +
                         "OUTPUT inserted.IdProduct\n" +
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
            return -100;

        }
    }

    public async Task<int> AddProductProcedure(ProductDTO productDto)
    {
        await using SqlConnection conn = new SqlConnection(_connectionString);
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

    public async Task<bool> DoesProductExist(int idProduct, SqlConnection sqlConnection, SqlTransaction sqlTransaction)
    {
        string sql = "SELECT COUNT(*)\n" +
                     "FROM Product\n" +
                     "WHERE IdProduct = @idProduct";
        
        await using SqlCommand cmd = new SqlCommand(sql, sqlConnection, sqlTransaction);

        cmd.Parameters.AddWithValue("@idProduct", idProduct);

        var result = await cmd.ExecuteScalarAsync();
        
        
        return Convert.ToInt32(result) > 0;
    }

    public async Task<bool> DoesWarehouseExist(int idWarehouse, SqlConnection sqlConnection, SqlTransaction sqlTransaction)
    {
        string sql = "SELECT COUNT(*)\n" +
                     "FROM Warehouse\n" +
                     "WHERE IdWarehouse = @idWarehouse";
        
        await using SqlCommand cmd = new SqlCommand(sql, sqlConnection, sqlTransaction);

        cmd.Parameters.AddWithValue("@idWarehouse", idWarehouse);

        var result = await cmd.ExecuteScalarAsync();
        
        
        return Convert.ToInt32(result) > 0;
    }

    public async Task<bool> CanAddToOrder(int idProduct, int amount, SqlConnection sqlConnection, SqlTransaction sqlTransaction)
    {
        string sql = "SELECT COUNT(*)\n" +
                     "FROM [Order]\n" +
                     "WHERE IdProduct = @idProduct AND Amount >= @amount";
        
        await using SqlCommand cmd = new SqlCommand(sql, sqlConnection, sqlTransaction);

        cmd.Parameters.AddWithValue("@idProduct", idProduct);
        cmd.Parameters.AddWithValue("@amount", amount);

        var result = await cmd.ExecuteScalarAsync();
        
        
        return Convert.ToInt32(result) > 0;
    }

    public async Task<bool> AlreadyDone(int idOrder, SqlConnection sqlConnection, SqlTransaction sqlTransaction)
    {
        string sql = "SELECT COUNT(*)\n" +
                     "FROM Product_Warehouse\n" +
                     "WHERE IdOrder = @idOrder";
        
        await using SqlCommand cmd = new SqlCommand(sql, sqlConnection, sqlTransaction);

        cmd.Parameters.AddWithValue("@idOrder", idOrder);

        var result = await cmd.ExecuteScalarAsync();
        
        
        return Convert.ToInt32(result) > 0;
    }

    public async Task<int> FindIdOrder(ProductDTO productDto, SqlConnection sqlConnection, SqlTransaction sqlTransaction)
    {
        string sql = "SELECT IdOrder\n" +
                     "FROM [Order]\n" +
                     "WHERE IdProduct = @IdProduct";
        
        await using SqlCommand cmd = new SqlCommand(sql, sqlConnection, sqlTransaction);

        cmd.Parameters.AddWithValue("@idProduct", productDto.IdProduct);


        return Convert.ToInt32(await cmd.ExecuteScalarAsync());
    }

    public async Task<int> UpdateOrderFullilledAt(int idOrder, SqlConnection sqlConnection, SqlTransaction sqlTransaction)
    {
        string sql = "UPDATE [Order]\n" +
                     "SET FulfilledAt = getdate()\n" +
                     "WHERE IdOrder = @idOrder";
        
        await using SqlCommand cmd = new SqlCommand(sql, sqlConnection, sqlTransaction);

        cmd.Parameters.AddWithValue("@idOrder", idOrder);

        
        return await cmd.ExecuteNonQueryAsync();
    }

    public async Task<Decimal> GetProductPrice(int idProduct, SqlConnection sqlConnection, SqlTransaction sqlTransaction)
    {
        string sql = "SELECT Price\n" +
                     "FROM Product\n" +
                     "WHERE IdProduct = @idProduct";
        
        await using SqlCommand cmd = new SqlCommand(sql, sqlConnection, sqlTransaction);

        cmd.Parameters.AddWithValue("@idProduct", idProduct);


        return Convert.ToDecimal(await cmd.ExecuteScalarAsync());
    }
}