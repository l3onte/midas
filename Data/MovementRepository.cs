using System.Data;
using System.Data.Common;
using System.Runtime;
using midasMVC.Models;
using midasMVC.Models.ViewModels;
using MySqlConnector;

namespace midasMVC.Data;

public class MovementRepository
{
    private readonly string _connectionString;

    public MovementRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("midas_db")
            ?? throw new InvalidOperationException("No se encontro la cadena midas");
    }

    public async Task<List<Account>> GetAccountsByUserIdAsync(int userId)
    {
        var list = new List<Account>();
        const string sql = "SELECT id, name FROM accounts WHERE user_id = @userId;";

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@userId", userId);

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new Account { Id = reader.GetInt32("id"), Name = reader.GetString("name") });
        }
        return list;
    }
    
    public async Task<List<Movement>> GetMovementsByUserIdAsync(int userId)
    {
        var movements = new List<Movement>();

        var sql = @"
            SELECT 
                m.id, m.user_id, m.account_id, m.movement_categorie_id, m.movement_type_id,
                m.description, m.amount, m.created_at,
                a.name AS account_name,
                mc.name AS movement_categorie_name,
                mt.name AS movement_type_name
            FROM movements m
            JOIN users u ON u.id = m.user_id
            JOIN accounts a ON a.id = m.account_id
            JOIN movement_categories mc ON mc.id = m.movement_categorie_id
            JOIN movements_type mt ON mt.id = m.movement_type_id
            WHERE m.user_id = @userId
            ORDER BY m.created_at ASC, m.id ASC; -- IMPORTANTE: Orden cronológico ascendente
        ";

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@userId", userId);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var movement = new Movement
            {
                Id = reader.GetInt32("id"),
                User_id = reader.GetInt32("user_id"),
                Account_id = reader.GetInt32("account_id"),
                Movement_categorie_id = reader.GetInt32("movement_categorie_id"),
                Movement_type_id = reader.GetInt32("movement_type_id"),
                Description = reader.GetString("description"),
                Amount = reader.GetDecimal("amount"),
                Created_at = reader.GetDateTime("created_at"),
                Account = new Account { Id = reader.GetInt32("account_id"), Name = reader.GetString("account_name") },
                MovementCategory = new MovementCategory { Id = reader.GetInt32("movement_categorie_id"), Name = reader.GetString("movement_categorie_name") },
                MovementType = new MovementType { Id = reader.GetInt32("movement_type_id"), Name = reader.GetString("movement_type_name") }
            };

            movements.Add(movement);
        }

        decimal runningBalance = 0m;
        foreach (var mov in movements)
        {
            bool esIngreso = mov.MovementType?.Name?.Equals("Ingreso", StringComparison.OrdinalIgnoreCase) ?? false;
            runningBalance += esIngreso ? mov.Amount : -mov.Amount;
            mov.RunningBalance = runningBalance;
        }


        movements.Reverse();

        return movements;
    }

    public async Task<bool> CreateMovementAsync(Movement movement)
    {
        var insertMovementSql = @"
            INSERT INTO movements (user_id, account_id, movement_categorie_id, movement_type_id, description, amount, created_at) 
            VALUES (@userId, @accountId, @movementCategorieId, @movementTypeId, @description, @amount, NOW());
        ";

        var updateBalanceSql = @"
            UPDATE accounts 
            SET balance = balance + CASE 
                WHEN @movementTypeId = 1 THEN @amount 
                ELSE -@amount 
            END
            WHERE id = @accountId AND user_id = @userId;
        ";

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var transaction = await connection.BeginTransactionAsync();

        try
        {
            await using var commandMovement = new MySqlCommand(insertMovementSql, connection, transaction);
            commandMovement.Parameters.AddWithValue("@userId", movement.User_id);
            commandMovement.Parameters.AddWithValue("@accountId", movement.Account_id);
            commandMovement.Parameters.AddWithValue("@movementCategorieId", movement.Movement_categorie_id);
            commandMovement.Parameters.AddWithValue("@movementTypeId", movement.Movement_type_id);
            commandMovement.Parameters.AddWithValue("@description", movement.Description ?? string.Empty);
            commandMovement.Parameters.AddWithValue("@amount", movement.Amount);
            
            await commandMovement.ExecuteNonQueryAsync();

            await using var commandBalance = new MySqlCommand(updateBalanceSql, connection, transaction);
            commandBalance.Parameters.AddWithValue("@userId", movement.User_id);
            commandBalance.Parameters.AddWithValue("@accountId", movement.Account_id);
            commandBalance.Parameters.AddWithValue("@movementTypeId", movement.Movement_type_id);
            commandBalance.Parameters.AddWithValue("@amount", movement.Amount);

            await commandBalance.ExecuteNonQueryAsync();

            await transaction.CommitAsync();
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<List<MovementCategory>> GetCategoriesAsync(int userId)
    {
        var list = new List<MovementCategory>();
        const string sql = "SELECT id, name FROM movement_categories WHERE user_id = @userId;";

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@userId", userId);

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new MovementCategory { Id = reader.GetInt32("id"), Name = reader.GetString("name") });
        }
        return list;
    }

    public async Task<List<MovementType>> GetMovementTypesAsync()
    {
        var list = new List<MovementType>();
        const string sql = "SELECT id, name FROM movements_type;";

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = new MySqlCommand(sql, connection);

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new MovementType { Id = reader.GetInt32("id"), Name = reader.GetString("name") });
        }
        return list;
    }

    public async Task<decimal> GetTotalBalanceByUserId(int userId)
    {
        const string sql = @"
        SELECT COALESCE(SUM(
            CASE 
                WHEN LOWER(mt.name) = 'ingreso' THEN m.amount 
                ELSE -m.amount 
            END
        ), 0)
        FROM movements m
        JOIN movements_type mt ON mt.id = m.movement_type_id
        WHERE m.user_id = @userId;";

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@userId", userId);

        var result = await command.ExecuteScalarAsync();
        return Convert.ToDecimal(result);
    }


}