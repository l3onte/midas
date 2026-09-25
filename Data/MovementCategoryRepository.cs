using midasMVC.Models.ViewModels;
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

    public async Task<List<MovementCategoryViewModel>> GetMovementCategoriesByUserWithBudgetsAmountAsync(int userId)
    {
        var list = new List<MovementCategoryViewModel>();

        const string sql = @"
            SELECT
                mc.id,
                mc.user_id,
                mc.name,
                b.amount AS budget_amount
            FROM movement_categories mc
            LEFT JOIN budgets b
                ON b.category_id = mc.id
                AND b.user_id = mc.user_id
                AND b.status = 1
                AND NOW() BETWEEN b.start_date AND b.end_date
            WHERE mc.user_id = @userId;
        ";

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@userId", userId);

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            list.Add(new MovementCategoryViewModel
            {
                Id = reader.GetInt32("id"),
                User_id = reader.GetInt32("user_id"),
                Name = reader.GetString("name"),
                BudgetAmount = reader.IsDBNull(reader.GetOrdinal("budget_amount"))
                    ? null
                    : reader.GetDecimal("budget_amount")
            });
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

    public async Task<bool> DeleteMovementCategoryAsync(int categorieId)
    {
        var sql = @"
            DELETE FROM movement_categories WHERE id = @categorieId
        ";

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@categorieId", categorieId);

        int rowsAffected= await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }


}