using System.Data;
using midasMVC.Models;
using MySqlConnector;

namespace midasMVC.Data;

public class UserRepository
{
    private readonly string _connectionString;

    public UserRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("midas_db") 
            ?? throw new InvalidOperationException("No se encontro la cadena midas_db");
    }

    public async Task<List<User>> GetUsersAsync()
    {
        var users = new List<User>();

        const string sql = @"
            SELECT 
                u.id, 
                u.rol_id, 
                u.name, 
                u.last_name, 
                u.email, 
                u.phone,
                u.status,
                u.created_at,
                u.updated_at,
                r.id AS role_id,
                r.name AS role_name,
                r.description AS role_description,
                r.status AS role_status
            FROM users u
            INNER JOIN roles r ON u.rol_id = r.id";

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new MySqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            users.Add(new User
            {
                Id = reader.GetInt32("id"),
                Role_id = reader.GetInt32("rol_id"),
                Name = reader.GetString("name"),
                Last_name = reader.GetString("last_name"),
                Email = reader.GetString("email"),
                Phone = reader.IsDBNull(reader.GetOrdinal("phone")) ? string.Empty : reader.GetString("phone"),
                Status = reader.GetBoolean("status"),
                Created_at = reader.GetDateTime("created_at"),
                Updated_at = reader.GetDateTime("updated_at"),
                Role = new Role
                {
                    Id = reader.GetInt32("role_id"),
                    Name = reader.GetString("role_name"),
                    Description = reader.IsDBNull(reader.GetOrdinal("role_description")) ? string.Empty : reader.GetString("role_description"),
                    Status = reader.GetBoolean("role_status")
                } 
            });
        }

        return users;
    }

    public async Task<int> CreateUserAsync(User user)
    {
        const string sql = @"
            INSERT INTO users (rol_id, name, last_name, email, password, phone, status, created_at, updated_at)
            VALUES (@rol_id, @name, @last_name, @email, @password, @phone, @status, NOW(), NOW());
            SELECT LAST_INSERT_ID();
        ";

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@rol_id", user.Role_id);
        command.Parameters.AddWithValue("@name", user.Name);
        command.Parameters.AddWithValue("@last_name", user.Last_name);
        command.Parameters.AddWithValue("@email", user.Email);
        command.Parameters.AddWithValue("@password", user.Password ?? string.Empty);
        command.Parameters.AddWithValue("@phone", string.IsNullOrWhiteSpace(user.Phone) ? (object)DBNull.Value : user.Phone);
        command.Parameters.AddWithValue("@status", user.Status ? 1 : 0);

        var newId = Convert.ToInt32(await command.ExecuteScalarAsync());
        return newId;
    }

    public async Task<bool> UpdateUserAsync(int id, User user)
    {
        const string sql = @"
            UPDATE users 
            SET rol_id = @rolId,
                name = @name,
                last_name = @lastName,
                email = @email,
                phone = @phone,
                status = @status,
                updated_at = NOW()
            WHERE id = @id;
        ";

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);
        command.Parameters.AddWithValue("@rolId", user.Role_id);
        command.Parameters.AddWithValue("@name", user.Name);
        command.Parameters.AddWithValue("@lastName", user.Last_name);
        command.Parameters.AddWithValue("@email", user.Email.Trim());
        command.Parameters.AddWithValue("@phone", string.IsNullOrWhiteSpace(user.Phone) ? (object)DBNull.Value : user.Phone);
        command.Parameters.AddWithValue("@status", user.Status ? 1 : 0);

        int rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        const string sql = @"
            SELECT 
                u.id, 
                u.rol_id, 
                u.name, 
                u.last_name, 
                u.email, 
                u.password, 
                u.phone,
                u.status,
                u.created_at,
                u.updated_at,
                r.id AS role_id,
                r.name AS role_name,
                r.description AS role_description,
                r.status AS role_status
            FROM users u
            INNER JOIN roles r ON u.rol_id = r.id
            WHERE u.email = @email
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
            Id = reader.GetInt32("id"),
            Role_id = reader.GetInt32("rol_id"),
            Name = reader.GetString("name"),
            Last_name = reader.GetString("last_name"),
            Email = reader.GetString("email"),
            Password = reader.GetString("password"),
            Phone = reader.IsDBNull(reader.GetOrdinal("phone")) ? string.Empty : reader.GetString("phone"),
            Status = reader.GetBoolean("status"),
            Created_at = reader.GetDateTime("created_at"),
            Updated_at = reader.GetDateTime("updated_at"),
            Role = new Role
            {
                Id = reader.GetInt32("role_id"),
                Name = reader.GetString("role_name"),
                Description = reader.IsDBNull(reader.GetOrdinal("role_description")) ? string.Empty : reader.GetString("role_description"),
                Status = reader.GetBoolean("role_status")
            }
        };
    }
    
    public async Task UpdatePasswordAsync(int userId, string newHash)
    {
        const string sql = "UPDATE users SET password = @password WHERE id = @id;";

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@password", newHash);
        command.Parameters.AddWithValue("@id", userId);

        await command.ExecuteNonQueryAsync();
    }
}