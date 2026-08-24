using MySqlConnector;

namespace midasMVC.Models;

public class MovementCategoryRepository
{
    private readonly string _connectionString;

    public MovementCategoryRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("midas_db")
            ?? throw new InvalidOperationException("No se encontro la cadena midas_db");

    }

    public async Task<List<MovementCategory>> GetMovementCategoriesByUserIdAsync(int userId)
    {
        var list = new List<MovementCategory>();
        var sql = @"
            SELECT 
                id, 
                user_id, 
                name 
            FROM movement_categories 
            WHERE user_id = @userId;  
        ";

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@userId", userId);

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            list.Add(new MovementCategory { Id = reader.GetInt32("id"), User_id = userId, Name = reader.GetString("name") });
        }

        return list;
    }

    public async Task<bool> CreateMovementCategoryAsync(MovementCategory movementCategorie)
    {
        var sql = @"
            INSERT INTO movement_categories (user_id, name) VALUES (@userId, @name);
        ";

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@userId", movementCategorie.User_id);
        command.Parameters.AddWithValue("@name", movementCategorie.Name);

        int rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<bool> UpdateMovementCategoryAsync(int categorieId, MovementCategory movementCategory)
    {
        var sql = @"
            UPDATE movement_categories
            SET name = @name
            WHERE id = @categorieId;
        ";

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@name", movementCategory.Name);
        command.Parameters.AddWithValue("@categorieId", categorieId);

        int rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }


}