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
        Guid CausationEventId,
        CancellationToken CancellationToken)
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
            CausationEventId,
            TriggeredAt = DateTime.UtcNow
        };

        await connection.OpenAsync(CancellationToken);

        await connection.ExecuteAsync(
            new CommandDefinition(
                sql,
                parameters,
                cancellationToken: CancellationToken));
    }

    public async Task<bool> HasTriggeredRecentlyAsync(
        Guid AutomationId,
        DateTime Cooldown,
        CancellationToken CancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
        SELECT CAST(
            CASE
                WHEN EXISTS (
                    SELECT 1
                    FROM AutomationHistory
                    WHERE AutomationId = @AutomationId
                      AND TriggeredAt >= @Cooldown
                )
                THEN 1
                ELSE 0
            END
        AS bit);
        """;

        var parameters = new
        {
            AutomationId,
            Cooldown
        };

        await connection.OpenAsync(CancellationToken);

        return await connection.QuerySingleAsync<bool>(
            new CommandDefinition(
                sql,
                parameters,
                cancellationToken: CancellationToken));
    }

    public async Task<bool> HasCausationEventIdAsync(
        Guid AutomationId,
        Guid CausationEventId,
        CancellationToken CancellationToken)
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
                )
                THEN 1
                ELSE 0
            END
        AS bit)
        """;

        var parameters = new
        {
            AutomationId,
            CausationEventId
        };

        await connection.OpenAsync(CancellationToken);

        return await connection.QuerySingleAsync<bool>(
            new CommandDefinition(
                sql,
                parameters,
                cancellationToken: CancellationToken));
    }
}