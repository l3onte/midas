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
                mc.name AS category_name,

                COALESCE(
                    SUM(
                        CASE
                            WHEN m.movement_type_id <> 1
                            THEN m.amount
                            ELSE 0
                        END
                    ),
                    0
                ) AS spent

            FROM budgets b

            INNER JOIN movement_categories mc
                ON mc.id = b.category_id

            LEFT JOIN movements m
                ON m.user_id = b.user_id
                AND m.movement_categorie_id = b.category_id
                AND m.created_at >= b.start_date
                AND m.created_at < DATE_ADD(b.end_date, INTERVAL 1 DAY)

            WHERE b.user_id = @userId

            GROUP BY
                b.id,
                b.user_id,
                b.category_id,
                b.amount,
                b.start_date,
                b.end_date,
                b.status,
                mc.id,
                mc.name

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

                Spent = reader.GetDecimal("spent"),

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