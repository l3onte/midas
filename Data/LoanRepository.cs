using midasMVC.Models;
using MySqlConnector;

public class LoanRepository
{
    private readonly string _connectionString;

    public LoanRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("midas_db")
            ?? throw new InvalidOperationException(
                "We can't find the string connection"
            );
    }

    public async Task<List<Loans>> GetLoansByUserIdAsync(int userId)
    {
        var loans = new List<Loans>();

        const string sql = @"
            SELECT
                id,
                user_id,
                person_name,
                amount,
                type,
                interest_rate,
                description,
                start_date,
                due_date,
                status
            FROM loans
            WHERE user_id = @userId
            AND status = 1;
        ";

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new MySqlCommand(sql, connection);

        command.Parameters.AddWithValue("@userId", userId);

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            loans.Add(new Loans
            {
                Id = reader.GetInt32("id"),
                UserId = reader.GetInt32("user_id"),
                PersonName = reader.GetString("person_name"),
                Amount = reader.GetDecimal("amount"),

                Type = Enum.Parse<LoanType>(
                    reader.GetString("type"),
                    true
                ),

                InterestRate = reader.GetDecimal("interest_rate"),
                Description = reader.GetString("description"),

                StartDate = reader.GetDateOnly("start_date"),

                DueDate = reader.IsDBNull(
                    reader.GetOrdinal("due_date")
                )
                    ? null
                    : reader.GetDateOnly("due_date"),

                Status = reader.GetBoolean("status")
            });
        }

        return loans;
    }

    public async Task<bool> CreateLoanAsync(Loans loan)
    {
        const string sql = @"
            INSERT INTO loans
            (
                user_id,
                person_name,
                amount,
                type,
                interest_rate,
                description,
                start_date,
                due_date,
                status
            )
            VALUES
            (
                @userId,
                @personName,
                @amount,
                @type,
                @interestRate,
                @description,
                @startDate,
                @dueDate,
                @status
            );
        ";

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new MySqlCommand(sql, connection);

        command.Parameters.AddWithValue("@userId", loan.UserId);
        command.Parameters.AddWithValue("@personName", loan.PersonName);
        command.Parameters.AddWithValue("@amount", loan.Amount);

        command.Parameters.AddWithValue(
            "@type",
            loan.Type == LoanType.Given
                ? "given"
                : "received"
        );

        command.Parameters.AddWithValue(
            "@interestRate",
            loan.InterestRate
        );

        command.Parameters.AddWithValue(
            "@description",
            loan.Description
        );

        command.Parameters.AddWithValue(
            "@startDate",
            loan.StartDate
        );

        command.Parameters.AddWithValue(
            "@dueDate",
            loan.DueDate.HasValue
                ? loan.DueDate.Value
                : DBNull.Value
        );

        command.Parameters.AddWithValue(
            "@status",
            loan.Status
        );

        int rowsAffected = await command.ExecuteNonQueryAsync();

        return rowsAffected > 0;
    }

    public async Task<bool> UpdateLoanAsyncById(Loans loan)
    {
        const string sql = @"
            UPDATE loans
            SET
                person_name = @personName,
                amount = @amount,
                type = @type,
                interest_rate = @interestRate,
                description = @description,
                start_date = @startDate,
                due_date = @dueDate,
                status = @status
            WHERE user_id = @userId
            AND id = @loanId;
        ";

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new MySqlCommand(sql, connection);

        command.Parameters.AddWithValue("@personName", loan.PersonName);
        command.Parameters.AddWithValue("@amount", loan.Amount);

        command.Parameters.AddWithValue(
            "@type",
            loan.Type == LoanType.Given
                ? "given"
                : "received"
        );

        command.Parameters.AddWithValue(
            "@interestRate",
            loan.InterestRate
        );

        command.Parameters.AddWithValue(
            "@description",
            loan.Description
        );

        command.Parameters.AddWithValue(
            "@startDate",
            loan.StartDate
        );

        command.Parameters.AddWithValue(
            "@dueDate",
            loan.DueDate.HasValue
                ? loan.DueDate.Value
                : DBNull.Value
        );

        command.Parameters.AddWithValue("@status", loan.Status);

        command.Parameters.AddWithValue("@userId", loan.UserId);
        command.Parameters.AddWithValue("@loanId", loan.Id);

        int rowsAffected = await command.ExecuteNonQueryAsync();

        return rowsAffected > 0;
    }

    public async Task<List<Loans>> GetLoansByUserIdAndLoanTypeAsync(int userId)
    {
        var Loans = new List<Loans>();

        const string sql = @"
            SELECT
                id,
                user_id,
                person_name,
                amount,
                type,
                interest_rate,
                description,
                start_date,
                due_date,
                status
            FROM loans
            WHERE user_id = @userId
            AND type = 'received'
            AND status = 1;
        ";

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@userId", userId);

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            Loans.Add(new Loans
            {
                Id = reader.GetInt32("id"),
                UserId = reader.GetInt32("user_id"),
                PersonName = reader.GetString("person_name"),
                Amount = reader.GetDecimal("amount"),

                Type = Enum.Parse<LoanType>(
                    reader.GetString("type"),
                    true
                ),

                InterestRate = reader.GetDecimal("interest_rate"),
                Description = reader.GetString("description"),

                StartDate = reader.GetDateOnly("start_date"),

                DueDate = reader.IsDBNull(
                    reader.GetOrdinal("due_date")
                )
                    ? null
                    : reader.GetDateOnly("due_date"),

                Status = reader.GetBoolean("status")
            });
        }

        return Loans;
    }

    public async Task<List<LoanPaymentOption>> GetLoansForPaymentAsync(int userId)
    {
        var loans = new List<LoanPaymentOption>();

        const string sql = @"
            SELECT
                l.id,
                l.person_name,
                l.amount,
                l.type,

                COALESCE(
                    SUM(lph.amount),
                    0
                ) AS paid_amount,

                (
                    l.amount -
                    COALESCE(SUM(lph.amount), 0)
                ) AS remaining_amount

            FROM loans l

            LEFT JOIN loan_payment_history lph
                ON lph.loan_id = l.id

            WHERE l.user_id = @userId
            AND l.status = 1

            GROUP BY
                l.id,
                l.person_name,
                l.amount,
                l.type

            HAVING
                (
                    l.amount -
                    COALESCE(SUM(lph.amount), 0)
                ) > 0

            ORDER BY l.person_name;
        ";

        await using var connection =
            new MySqlConnection(_connectionString);

        await connection.OpenAsync();

        await using var command =
            new MySqlCommand(sql, connection);

        command.Parameters.AddWithValue("@userId", userId);

        await using var reader =
            await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            loans.Add(new LoanPaymentOption
            {
                Id = reader.GetInt32("id"),

                PersonName = reader.GetString("person_name"),

                Amount = reader.GetDecimal("amount"),

                Type = Enum.Parse<LoanType>(
                    reader.GetString("type"),
                    true
                ),

                PaidAmount = reader.GetDecimal("paid_amount"),

                RemainingAmount =
                    reader.GetDecimal("remaining_amount")
            });
        }

        return loans;
    }
}