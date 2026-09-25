using AutomationStation.Application.Abstractions;
using AutomationStation.Infrastructure.Database;
using Dapper;

namespace AutomationStation.Infrastructure.Persistence;

public sealed class HistoryRepository(
    DbConnectionFactory connectionFactory)
    : IHistoryRepository
{
    private readonly DbConnectionFactory _connectionFactory = connectionFactory;

    public async Task CreateAutomationTimestampAsync(
        Guid AutomationId,
        Guid causationEventId,
        CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
                INSERT INTO AutomationHistory (Id, AutomationId, CausationEventId, TriggeredAt) 
                VALUES (@Id, @AutomationId, @CausationEventId, @TriggeredAt)
                """;

        var parameters = new
        {
            Id = Guid.NewGuid(),
            AutomationId,
            CausationEventId = causationEventId,
            TriggeredAt = DateTime.UtcNow
        };

        await connection.OpenAsync(cancellationToken);

        await connection.ExecuteAsync(
            new CommandDefinition(
                sql,
                parameters,
                cancellationToken: cancellationToken));
    }

    public async Task<bool> HasTriggeredRecentlyAsync(
        Guid AutomationId,
        Guid causationEventId,
        DateTime cooldown,
        CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
        SELECT CAST(
            CASE
                WHEN EXISTS (
                    SELECT 1
                    FROM AutomationHistory
                    WHERE AutomationId = @AutomationId
                      AND CausationEventId = @CausationEventId
                      AND TriggeredAt >= @Cooldown
                )
                THEN 1
                ELSE 0
            END
        AS bit);
        """;

        var parameters = new 
        { 
            AutomationId, CausationEventId = causationEventId, Cooldown = cooldown 
        };

        await connection.OpenAsync(cancellationToken);

        return await connection.QuerySingleAsync<bool>(
            new CommandDefinition(
                sql,
                parameters,
                cancellationToken: cancellationToken));
    }
}