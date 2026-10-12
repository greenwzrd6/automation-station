using AutomationStation.Application.Abstractions;
using AutomationStation.Infrastructure.Database;
using Dapper;

namespace AutomationStation.Infrastructure.Persistence;

public sealed class AutomationExecutorRepository(
    DbConnectionFactory connectionFactory)
    : IAutomationExecutorRepository
{
    public async Task<bool> IsEnabledAsync(
        Guid executorId,
        CancellationToken cancellationToken)
    {
        await using var connection =
            connectionFactory.CreateConnection();

        const string sql = """
            SELECT CAST(
                CASE WHEN EXISTS
                (
                    SELECT 1
                    FROM AutomationExecutors
                    WHERE Id = @ExecutorId
                      AND Enabled = 1
                )
                THEN 1
                ELSE 0
                END
                AS BIT);
            """;

        return await connection.ExecuteScalarAsync<bool>(
            new CommandDefinition(
                sql,
                new { ExecutorId = executorId },
                cancellationToken: cancellationToken));
    }
}