using midasMVC.Models;
using MySqlConnector;

public class CuentasRepository
{
    private readonly string _connectionString;
    public CuentasRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("midas_db")
            ?? throw new InvalidOperationException("No se encontro la cadena midas_db");

    }

    public async Task<List<Account>> GetAccountsByUserIdAsync(int userId)
    {
        var list = new List<Account>();
        var sql = @"
            SELECT 
                id,
                user_id, 
                name, 
                balance, 
                account_type, 
                status 
            FROM accounts 
            WHERE user_id = @userId;
        ";

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@userId", userId);

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            string rawAccountType = reader.GetString("account_type").Replace("_", "");

            if (!Enum.TryParse<AccountType>(rawAccountType, ignoreCase: true, out var accountType))
            {
                accountType = AccountType.Efectivo;
            }

            list.Add(new Account
            {
                Id = reader.GetInt32("id"),
                User_id = reader.GetInt32("user_id"),
                Name = reader.GetString("name"),
                Balance = reader.GetDecimal("balance"),
                Account_type = accountType,
                Status = reader.GetBoolean("status")
            });
        }

        return list;
    }

    public async Task<bool> CreateAccountAsync(Account account)
    {
        var sql = @"
            INSERT INTO accounts (user_id, name, balance, account_type, status)
            VALUES (@userId, @name, @balance, @accountType, @status);
        ";

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@userId", account.User_id);
        command.Parameters.AddWithValue("@name", account.Name);
        command.Parameters.AddWithValue("@balance", account.Balance);
        command.Parameters.AddWithValue("@accountType", account.Account_type.ToString().ToLower());
        command.Parameters.AddWithValue("@status", account.Status);

        int rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<bool> EditAccountAsync(int accountId, int userId, Account account)
    {
        var sql = @"
            UPDATE accounts
            SET name = @name, status = @status
            WHERE id = @accountId AND user_id = @userId;
        ";

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@accountId", accountId);
        command.Parameters.AddWithValue("@userId", userId);
        command.Parameters.AddWithValue("@name", account.Name);
        command.Parameters.AddWithValue("@status", account.Status);

        int rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }
}