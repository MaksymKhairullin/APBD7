using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using DB_API.DTOs;
using DB_API.Models;

namespace DB_API.Services;


public interface IDbService
{
    Task<bool> DoesProductExist(int productId);
    Task<bool> DoesWarehouseExist(int warehouseId);
    Task<bool> IsOrderFulfilled(int productId, int amount);
    
    //Task Update(int productId);
    Task<int> InsertProductWarehouse(int productId, int warehouseId, DateTime createdAt);
}



public class DbService : IDbService
{
    
    private readonly string _connectionString;
    public DbService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Default")
                            ?? throw new ArgumentNullException(nameof(configuration));
    }
    
    public async Task<bool> DoesProductExist(int productId)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM Product WHERE IdProduct = @1";
            command.Parameters.AddWithValue("@1", productId);
            await connection.OpenAsync();
            int count = Convert.ToInt32(await command.ExecuteScalarAsync());
            return count > 0;
        }
    }

    public async Task<bool> DoesWarehouseExist(int warehouseId)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM Warehouse WHERE IdWarehouse = @1";
            command.Parameters.AddWithValue("@1", warehouseId);
            await connection.OpenAsync();
            int count = Convert.ToInt32(await command.ExecuteScalarAsync());
            return count > 0;
        }
    }

    public async Task<bool> IsOrderFulfilled(int productId, int amount)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM [Order] WHERE IdProduct = @1 AND Amount = @2 AND FulfilledAt IS NULL";
            command.Parameters.AddWithValue("@1", productId);
            command.Parameters.AddWithValue("@2", amount);
            await connection.OpenAsync();
            int count = Convert.ToInt32(await command.ExecuteScalarAsync());
            return count > 0;
        }
    }
    

    // public async Task Update(int productId)
    // {
    //     using (var connection = new SqlConnection(_connectionString))
    //     {
    //         var command = connection.CreateCommand();
    //         command.CommandText = "UPDATE Product SET UpdatedAt = @UpdatedAt WHERE IdProduct = @ProductId";
    //         command.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow);
    //         command.Parameters.AddWithValue("@ProductId", productId);
    //         await connection.OpenAsync();
    //         await command.ExecuteNonQueryAsync();
    //     }
    // }

    public async Task<int> InsertProductWarehouse(int productId, int warehouseId, DateTime createdAt)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            var command = connection.CreateCommand();
            command.CommandText = "INSERT INTO Product_Warehouse (IdWarehouse, IdProduct, IdOrder, Amount, Price, CreatedAt) " +
                                  "SELECT @IdWarehouse, @IdProduct, o.IdOrder, o.Amount, o.Amount * p.Price, @CreatedAt FROM [Order] o" +
                                  "INNER JOIN Product p ON o.IdProduct = p.IdProduct WHERE o.IdProduct = @IdProduct AND o.FulfilledAt IS NOT NULL;" +
                                  "SELECT SCOPE_IDENTITY();";
            command.Parameters.AddWithValue("@IdProduct", productId);
            command.Parameters.AddWithValue("@IdWarehouse", warehouseId);
            command.Parameters.AddWithValue("@CreatedAt", createdAt);

            connection.Open();
            var transaction = connection.BeginTransaction();
            command.Transaction = transaction;

            try
            {
                var rowsAffected = await command.ExecuteNonQueryAsync();
                transaction.Commit();
                return rowsAffected;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw ex;
            }
        }
    }
}







    
