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
                INSERT INTO AutomationHistory (Id, AutomationId, TriggeredAt) 
                VALUES (@Id, @AutomationId, @TriggeredAt)
                """;

        var parameters = new
        {
            Id = Guid.NewGuid(),
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

    public async Task<DateTime?> HasTriggeredSinceAsync(
        Guid AutomationId,
        DateTime since,
        CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """

                    SELECT top 1 TriggeredAt
                    FROM AutomationHistory
                    WHERE AutomationId = @AutomationId
                     Order by TriggeredAt DESC

        """;

        var parameters = new 
        { 
            AutomationId, Since = since 
        };

        await connection.OpenAsync(cancellationToken);

        return await connection.QueryFirstAsync<DateTime?>(
            new CommandDefinition(
                sql,
                parameters,
                cancellationToken: cancellationToken));
    }
}