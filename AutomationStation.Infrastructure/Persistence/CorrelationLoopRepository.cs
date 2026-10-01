using Dapper;

using AutomationStation.Application.Abstractions;
using AutomationStation.Infrastructure.Database;

namespace AutomationStation.Infrastructure.Persistence;

public sealed class CorrelationLoopRepository(
    DbConnectionFactory connectionFactory)
    : ICorrelationLoopRepository
{
    private readonly DbConnectionFactory _connectionFactory = connectionFactory;
    public async Task<bool> IsBlockedAsync(
        Guid correlationId,
        Guid eventId,
        CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
            SET XACT_ABORT ON;

            BEGIN TRANSACTION;

            DECLARE @Count INT;
            DECLARE @Blocked BIT = 0;

            SELECT @Count = COUNT(*)
            FROM CorrelationLoopEvents WITH (UPDLOCK, HOLDLOCK)
            WHERE CorrelationId = @CorrelationId
              AND ReceivedAt >= DATEADD(MINUTE, -3, SYSUTCDATETIME());

            IF @Count >= 10
            BEGIN
                SET @Blocked = 1;
            END
            ELSE
            BEGIN
                INSERT INTO CorrelationLoopEvents (Id, CorrelationId, EventId, ReceivedAt)
                VALUES (@Id, @CorrelationId, @EventId, SYSUTCDATETIME());
            END

            COMMIT TRANSACTION;

            SELECT @Blocked;
            """;

        var parameters = new
        {
            Id = Guid.NewGuid(),
            CorrelationId = correlationId,
            EventId = eventId
        };

        await connection.OpenAsync(cancellationToken);

        return await connection.QuerySingleAsync<bool>(
            new CommandDefinition(
                sql,
                parameters,
                cancellationToken: cancellationToken));
    }

    public async Task CleanupOldEventsAsync(
        TimeSpan maxAge,
        CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
            DELETE FROM CorrelationLoopEvents
            WHERE ReceivedAt < @Cutoff;
            """;

        var parameters = new
        {
            Cutoff = DateTime.UtcNow - maxAge
        };

        await connection.OpenAsync(cancellationToken);

        await connection.ExecuteAsync(
            new CommandDefinition(
                sql,
                parameters,
                cancellationToken: cancellationToken));
    }
}
