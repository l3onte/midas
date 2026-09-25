using midasMVC.Models;
using MySqlConnector;

namespace midasMVC.Data;

public class BudgetRepository
{
    private readonly string _connectionString;

    public BudgetRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("midas_db")
            ?? throw new InvalidOperationException("We can't find the connection string");
    }

    public async Task<List<Budget>> GetBudgetsByUserIdAsync(int userId)
    {
        const string sql = @"
            SELECT
                b.id,
                b.user_id,
                b.category_id AS budget_category_id,
                b.amount,
                b.start_date,
                b.end_date,
                b.status,

                mc.id AS category_id,
                mc.name AS category_name

            FROM budgets b
            INNER JOIN movement_categories mc
                ON mc.id = b.category_id

            WHERE b.user_id = @userId
              AND b.status = 1
              
            ORDER BY b.start_date DESC;
        ";

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@userId", userId);

        await using var reader = await command.ExecuteReaderAsync();

        var budgets = new List<Budget>();

        while (await reader.ReadAsync())
        {
            var budget = new Budget
            {
                Id = reader.GetInt32("id"),
                UserId = reader.GetInt32("user_id"),
                CategoryId = reader.GetInt32("budget_category_id"),
                Amount = reader.GetDecimal("amount"),
                StartDate = reader.GetDateTime("start_date"),
                EndDate = reader.GetDateTime("end_date"),
                Status = reader.GetBoolean("status"),

                MovementCategory = new MovementCategory
                {
                    Id = reader.GetInt32("category_id"),
                    Name = reader.GetString("category_name")
                }
            };

            budgets.Add(budget);
        }

        return budgets;
    }

    public async Task<bool> CreateBudgetAsync(Budget budget)
    {
        const string sql = @"
            INSERT INTO budgets (user_id, category_id, amount, start_date, end_date, status)
            VALUES (@userId, @categoryId, @amount, @startDate, @endDate, @status);
        ";

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@userId", budget.UserId);
        command.Parameters.AddWithValue("@categoryId", budget.CategoryId);
        command.Parameters.AddWithValue("@amount", budget.Amount);
        command.Parameters.AddWithValue("@startDate", budget.StartDate);
        command.Parameters.AddWithValue("@endDate", budget.EndDate);
        command.Parameters.AddWithValue("@status", budget.Status);

        int rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<bool> UpdateBudgetAsync(int budgetId, int userId, Budget budget)
    {
        const string sql = @"
            UPDATE budgets
            SET
                category_id = @categoryId,
                amount = @amount,
                start_date = @startDate,
                end_date = @endDate,
                status = @status
            WHERE id = @budgetId
            AND user_id = @userId;
        ";

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new MySqlCommand(sql, connection);

        command.Parameters.AddWithValue("@budgetId", budgetId);
        command.Parameters.AddWithValue("@userId", userId);
        command.Parameters.AddWithValue("@categoryId", budget.CategoryId);
        command.Parameters.AddWithValue("@amount", budget.Amount);
        command.Parameters.AddWithValue("@startDate", budget.StartDate);
        command.Parameters.AddWithValue("@endDate", budget.EndDate);
        command.Parameters.AddWithValue("@status", budget.Status);

        int rowsAffected = await command.ExecuteNonQueryAsync();

        return rowsAffected > 0;
    }

    public async Task<bool> DeleteBudgetAsync(int budgetId, int userId)
    {
        const string sql = @"
            DELETE FROM budgets
            WHERE id = @budgetId
            AND user_id = @userId;
        ";

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@budgetId", budgetId);
        command.Parameters.AddWithValue("@userId", userId);

        int rowsAffected = await command.ExecuteNonQueryAsync();

        return rowsAffected > 0;
    }
}