using Automation.Application.Abstractions;
using Automation.Infrastructure.Database;
using Dapper;

namespace Automation.Infrastructure.Persistence;

public sealed class HistoryRepository(
    DbConnectionFactory connectionFactory)
    : IHistoryRepository
{
    private readonly DbConnectionFactory _connectionFactory = connectionFactory;

    public async Task CreateAutomationTimestampAsync(
        Guid AutomationId,
        CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
                INSERT INTO AutomationHistory (AutomationId, TriggeredAt) 
                VALUES (@AutomationId, @TriggeredAt)
                """;

        var parameters = new
        {
            AutomationId,
            TriggeredAt = DateTime.UtcNow
        };

        await connection.OpenAsync(cancellationToken);

        await connection.ExecuteAsync(
            new CommandDefinition(
                sql,
                parameters,
                cancellationToken: cancellationToken));
    }

    public async Task<bool> HasTriggeredSinceAsync(
        Guid AutomationId,
        DateTime since,
        CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
            SELECT CAST(
                CASE WHEN EXISTS (
                    SELECT 1
                    FROM AutomationHistory
                    WHERE AutomationId = @AutomationId
                     AND TriggeredAt >= @Since
                )
                THEN 1
                ELSE 0
                END
            AS BIT)
        """;

        var parameters = new 
        { 
            AutomationId, Since = since 
        };

        await connection.OpenAsync(cancellationToken);

        return await connection.ExecuteScalarAsync<bool>(
            new CommandDefinition(
                sql,
                parameters,
                cancellationToken: cancellationToken));
    }
}