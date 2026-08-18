using MySqlConnector;
using midasMVC.Models;
using System.Data;

namespace midasMVC.Data;

public class UserRepository
{
    private readonly string _connectionString;

    public UserRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("midas_db") ?? throw new InvalidOperationException("No se encontro la cadena midas_db");
    }

    public async Task<User?>GetByEmailAsync(string email)
    {
        const string sql = @"
            SELECT id, name, email, password, role_id, status
            FROM users
            WHERE email = @email
            LIMIT 1;       
        ";

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@email", email.Trim());

        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return null;

        return new User
        {
            Id = (int)reader.GetInt64("id"),
            Name = reader.GetString("emal"),
            Email = reader.GetString("email"),
            Password = reader.GetString("password"),
            Role_id = (int)reader.GetInt64("role_id"),
            Status = reader.GetBoolean("status")
        };
    }
}