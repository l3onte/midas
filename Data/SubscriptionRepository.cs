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

    public async Task<bool> CreatePaymentMethodAndPaySuscriptionAsync(
        PaymentMethod paymentMethod,
        int planSubscriptionIdSelected)
    {
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        const string validationSql = @"
            SELECT card_number
            FROM payment_method
            WHERE card_number = @cardNumber
            AND user_id = @userId
            LIMIT 1;
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
            const string sqlPaymentMethod = @"
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

            await using var commandPaymentMethod =
                new MySqlCommand(sqlPaymentMethod, connection, transaction);

            commandPaymentMethod.Parameters.AddWithValue("@userId", paymentMethod.UserId);
            commandPaymentMethod.Parameters.AddWithValue("@ownerName", paymentMethod.OwnerName);
            commandPaymentMethod.Parameters.AddWithValue("@ownerLastName", paymentMethod.OwnerLastName);
            commandPaymentMethod.Parameters.AddWithValue("@cardNumber", paymentMethod.CardNumber);
            commandPaymentMethod.Parameters.AddWithValue("@cvCode", paymentMethod.CvCode);
            commandPaymentMethod.Parameters.AddWithValue("@expirationDate", paymentMethod.ExpirationDate);

            var paymentMethodId =
                Convert.ToInt32(await commandPaymentMethod.ExecuteScalarAsync());

            const string sqlCheckSubscription = @"
                SELECT id
                FROM user_plan_subscription
                WHERE user_id = @userId
                LIMIT 1;
            ";

            await using var commandCheckSubscription =
                new MySqlCommand(sqlCheckSubscription, connection, transaction);

            commandCheckSubscription.Parameters.AddWithValue(
                "@userId",
                paymentMethod.UserId
            );

            var existingSubscription =
                await commandCheckSubscription.ExecuteScalarAsync();

            if (existingSubscription != null)
            {
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
                    new MySqlCommand(
                        sqlUpdateSubscription,
                        connection,
                        transaction
                    );

                commandUpdateSubscription.Parameters.AddWithValue(
                    "@paymentMethodId",
                    paymentMethodId
                );

                commandUpdateSubscription.Parameters.AddWithValue(
                    "@planId",
                    planSubscriptionIdSelected
                );

                commandUpdateSubscription.Parameters.AddWithValue(
                    "@startDate",
                    startDate
                );

                commandUpdateSubscription.Parameters.AddWithValue(
                    "@endDate",
                    endDate
                );

                commandUpdateSubscription.Parameters.AddWithValue(
                    "@userId",
                    paymentMethod.UserId
                );

                await commandUpdateSubscription.ExecuteNonQueryAsync();
            }
            else
            {
                const string sqlInsertSubscription = @"
                    INSERT INTO user_plan_subscription (
                        payment_method_id,
                        user_id,
                        plan_id,
                        start_date,
                        end_date,
                        status
                    )
                    VALUES (
                        @paymentMethodId,
                        @userId,
                        @planId,
                        @startDate,
                        @endDate,
                        1
                    );
                ";

                await using var commandInsertSubscription =
                    new MySqlCommand(
                        sqlInsertSubscription,
                        connection,
                        transaction
                    );

                commandInsertSubscription.Parameters.AddWithValue(
                    "@paymentMethodId",
                    paymentMethodId
                );

                commandInsertSubscription.Parameters.AddWithValue(
                    "@userId",
                    paymentMethod.UserId
                );

                commandInsertSubscription.Parameters.AddWithValue(
                    "@planId",
                    planSubscriptionIdSelected
                );

                commandInsertSubscription.Parameters.AddWithValue(
                    "@startDate",
                    startDate
                );

                commandInsertSubscription.Parameters.AddWithValue(
                    "@endDate",
                    endDate
                );

                await commandInsertSubscription.ExecuteNonQueryAsync();
            }

            const string sqlUpdateRole = @"
                UPDATE users
                SET
                    rol_id = 3
                WHERE id = @userId;
            ";

            await using var commandUpdateRole =
                new MySqlCommand(
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


    public async Task<List<Suscription>> GetSubscriptionInfoByUserIdAsync(int userId)
    {
        const string sql = @"
            SELECT
                ps.id,
                ps.payment_method_id,
                ps.user_id,
                ps.plan_id,
                ps.start_date,
                ps.end_date,
                ps.status,

                pm.id,
                pm.owner_name,
                pm.owner_last_name,
                pm.card_number,

                u.id,
                u.name,
                u.last_name,
                u.email,

                p.id,
                p.name
            FROM user_plan_subscription ps
            LEFT JOIN payment_method pm ON pm.id = ps.payment_method_id
            LEFT JOIN users u ON u.id = ps.user_id
            LEFT JOIN plans p ON p.id = ps.plan_id
            WHERE ps.user_id = @userId;
        ";

        var subscriptions = new List<Suscription>();

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@userId", userId);

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var subscription = new Suscription
            {
                Id = reader.GetInt32(0),
                PaymentMethodId = reader.GetInt32(1),
                UserId = reader.GetInt32(2),
                PlanId = reader.GetInt32(3),
                StartDate = reader.GetDateTime(4),
                EndDate = reader.GetDateTime(5),
                Status = reader.GetBoolean(6),

                PaymentMethod = reader.IsDBNull(7)
                    ? null
                    : new PaymentMethod
                    {
                        Id = reader.GetInt32(7),
                        OwnerName = reader.IsDBNull(8) ? "" : reader.GetString(8),
                        OwnerLastName = reader.IsDBNull(9) ? "" : reader.GetString(9),
                        CardNumber = reader.IsDBNull(10) ? "" : reader.GetString(10)
                    },

                User = reader.IsDBNull(11)
                    ? null
                    : new User
                    {
                        Id = reader.GetInt32(11),
                        Name = reader.IsDBNull(12) ? "" : reader.GetString(12),
                        Last_name = reader.IsDBNull(13) ? "" : reader.GetString(13),
                        Email = reader.IsDBNull(14) ? "" : reader.GetString(14)
                    },

                Plan = reader.IsDBNull(15)
                    ? null
                    : new SubscriptionPlan
                    {
                        Id = reader.GetInt32(15),
                        Name = reader.IsDBNull(16) ? "" : reader.GetString(16)
                    }
            };

            subscriptions.Add(subscription);
        }

        return subscriptions;
    }

}