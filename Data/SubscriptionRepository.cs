using midasMVC.Models;
using MySqlConnector;

namespace midasMVC.Data;

public class SubscriptionRepository
{
    private readonly string _connectionString;

    public SubscriptionRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("midas_db")
            ?? throw new OperationCanceledException("No se encontro la cadena midas");
    }


    public async Task<bool> CreatePaymentMethodAndPaySuscriptionAsync(PaymentMethod paymentMethod, int planSubscriptionIdSelected)
    {
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        const string validationSql = @"
            SELECT 
                card_number 
            FROM payment_method 
            WHERE card_number = @cardNumber 
            AND user_id = @userId
            LIMIT 1
        ";

        await using var commandValidation = new MySqlCommand(validationSql, connection);

        commandValidation.Parameters.AddWithValue("@userId", paymentMethod.UserId);
        commandValidation.Parameters.AddWithValue("@cardNumber", paymentMethod.CardNumber);

        var existingPaymentMethod = await commandValidation.ExecuteScalarAsync();

        if (existingPaymentMethod != null)
        {
            return false;
        }

        var startDate = DateTime.Now;

        var endDate = planSubscriptionIdSelected switch
        {
            2 => startDate.AddMonths(1),
            3 => startDate.AddYears(1),
            _ => throw new ArgumentException("Plan no válido")
        };
        await using var transaction = await connection.BeginTransactionAsync();

        try
        {
            const string sql = @"
                INSERT INTO payment_method (
                    user_id, 
                    owner_name, 
                    owner_last_name, 
                    card_number, 
                    cv_code, 
                    expiration_date, 
                    created_at
                ) 
                VALUES (
                    @userId, 
                    @ownerName, 
                    @ownerLastName, 
                    @cardNumber, 
                    @cvCode, 
                    @expirationDate, 
                    NOW()
                );

                SELECT LAST_INSERT_ID();                                 
            ";

            await using var commandPaymentMethod = new MySqlCommand(sql, connection, transaction);
            commandPaymentMethod.Parameters.AddWithValue("@userId", paymentMethod.UserId);
            commandPaymentMethod.Parameters.AddWithValue("@ownerName", paymentMethod.OwnerName);
            commandPaymentMethod.Parameters.AddWithValue("@ownerLastName", paymentMethod.OwnerLastName);
            commandPaymentMethod.Parameters.AddWithValue("@cardNumber", paymentMethod.CardNumber);
            commandPaymentMethod.Parameters.AddWithValue("@cvCode", paymentMethod.CvCode);
            commandPaymentMethod.Parameters.AddWithValue("@expirationDate", paymentMethod.ExpirationDate);

            var paymentMethodId = Convert.ToInt32(await commandPaymentMethod.ExecuteScalarAsync());

            const string sqlUpdateSubscription = @"
                UPDATE user_plan_subscription
                SET
                    payment_method_id = @paymentMethodId,
                    plan_id = @planId,
                    start_date = @startDate,
                    end_date = @endDate,
                    status = 1
                WHERE user_id = @userId;
            ";

            await using var commandUpdateSubscription =
                new MySqlCommand(sqlUpdateSubscription, connection, transaction);

            commandUpdateSubscription.Parameters.AddWithValue("@paymentMethodId", paymentMethodId);
            commandUpdateSubscription.Parameters.AddWithValue("@planId", planSubscriptionIdSelected);
            commandUpdateSubscription.Parameters.AddWithValue("@startDate", startDate);
            commandUpdateSubscription.Parameters.AddWithValue("@endDate", endDate);
            commandUpdateSubscription.Parameters.AddWithValue("@userId", paymentMethod.UserId);

            await commandUpdateSubscription.ExecuteNonQueryAsync();

            const string sqlUpdateRole = @"
                UPDATE users
                SET 
                    rol_id = 3
                WHERE id = @userId;
            ";

            await using var commandUpdateRole = new MySqlCommand(
                sqlUpdateRole,
                connection,
                transaction
            );

            commandUpdateRole.Parameters.AddWithValue(
                "@userId",
                paymentMethod.UserId
            );

            await commandUpdateRole.ExecuteNonQueryAsync();

            await transaction.CommitAsync();

            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> UpdatePaymentMethodAsync(PaymentMethod paymentMethod)
    {
        const string sql = @"
            UPDATE payment_method
            SET 
                owner_name = @ownerName,
                owner_last_name = @ownerLastName,
                card_number = @cardNumber,
                cv_code = @cvCode,
                expiration_date = @expirationDate
            WHERE user_id = @userId;
        ";

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@userId", paymentMethod.UserId);
        command.Parameters.AddWithValue("@ownerName", paymentMethod.OwnerName);
        command.Parameters.AddWithValue("@ownerLastName", paymentMethod.OwnerLastName);
        command.Parameters.AddWithValue("@cardNumber", paymentMethod.CardNumber);
        command.Parameters.AddWithValue("@cvCode", paymentMethod.CvCode);
        command.Parameters.AddWithValue("@expirationDate", paymentMethod.ExpirationDate);

        int rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<bool> DeletePaymentNethodAsync(int userId)
    {
        const string sql = @"
            DELETE FROM payment_method WHERE user_id = @userId;
        ";

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@userId", userId);

        int rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

}