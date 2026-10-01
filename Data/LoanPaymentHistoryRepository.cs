namespace midasMVC.Models;
using MySqlConnector;

public class LoanPaymentHistoryRepository
{
    private readonly string _connectionString;

    public LoanPaymentHistoryRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("midas_db")
                                ?? throw new InvalidOperationException("We cant find the connection string");

    }

    public async Task<List<LoanPaymentHistory>> GetPaymentHistoryByLoanIdAsync(int loanId)
    {
        var paymentHistory = new List<LoanPaymentHistory>();

        const string sql = @"
            SELECT
                id,
                loan_id,
                amount,
                payment_date
            FROM loan_payment_history
            WHERE loan_id = @loanId;
        ";

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@loanId", loanId);

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            paymentHistory.Add(new LoanPaymentHistory
            {
                Id = reader.GetInt32("id"),
                LoanId = reader.GetInt32("loan_id"),
                Amount = reader.GetDecimal("amount"),
                PaymentDate = reader.GetDateTime("payment_date")
            });
        }

        return paymentHistory;
    }
}