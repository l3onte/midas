using midasMVC.Models;

namespace MySqlConnector;

public class MetasRepository
{
    private readonly string _connectionString;

    public MetasRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("midas_db")
            ?? throw new InvalidOperationException("Cadena de conexion no encontrada");
    }

    public async Task<List<Goal>> GetGoalsByUserIdAsync(int userId)
    {
        var list = new List<Goal>();

        var sql = @"
            SELECT
                id,
                user_id,
                name,
                target_amount,
                current_amount,
                status
            FROM goals
            WHERE user_id = @userId;
        ";

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@userId", userId);

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new Goal
            {
                Id = reader.GetInt32("id"),
                User_id = reader.GetInt32("user_id"),
                Name = reader.GetString("name"),
                Target_amount = reader.GetDecimal("target_amount"),
                Current_amount = reader.GetDecimal("current_amount"),
                Status = reader.GetBoolean("status")
            });
        }

        return list;
    }

    public async Task<bool> CreateGoalAsync(int userId, Goal goal)
    {
        var sql = @"
            INSERT INTO goals (user_id, name, target_amount, current_amount, status)
            VALUES (@userId, @name, @targetAmount, @currentAmount, @status);
        ";

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@userId", userId);
        command.Parameters.AddWithValue("@name", goal.Name);
        command.Parameters.AddWithValue("@targetAmount", goal.Target_amount);
        command.Parameters.AddWithValue("@currentAmount", goal.Current_amount);
        command.Parameters.AddWithValue("@status", goal.Status);

        int rowsAffected = command.ExecuteNonQuery();
        return rowsAffected > 0;
    }

    public async Task<bool> UpdateGoalAsync(int goalId, Goal goal)
    {
        var sql = @"
            UPDATE goals
            SET name = @name, target_amount = @targetAmount, current_amount = @currentAmount, status = @status
            WHERE id = @goalId;
        ";

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@name", goal.Name);
        command.Parameters.AddWithValue("@targetAmount", goal.Target_amount);
        command.Parameters.AddWithValue("@currentAmount", goal.Current_amount);
        command.Parameters.AddWithValue("@status", goal.Status);
        command.Parameters.AddWithValue("@goalId", goalId);

        int rowsAffected = command.ExecuteNonQuery();
        return rowsAffected > 0;
    }
}