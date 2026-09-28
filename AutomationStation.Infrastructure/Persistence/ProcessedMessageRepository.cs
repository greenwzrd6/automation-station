using AutomationStation.Application.Abstractions;
using AutomationStation.Infrastructure.Database;
using Dapper;

namespace AutomationStation.Infrastructure.Persistence;

public sealed class ProcessedMessageRepository(
    DbConnectionFactory connectionFactory)
    : IProcessedMessageRepository
{
    private readonly DbConnectionFactory _connectionFactory =
        connectionFactory;

    public async Task<bool> HasProcessedAsync(
        Guid messageId,
        CancellationToken cancellationToken)
    {
        using var connection =
            _connectionFactory.CreateConnection();

        const string sql = """
            SELECT CAST(
                CASE WHEN EXISTS (
                    SELECT 1
                    FROM ProcessedMessages
                    WHERE MessageId = @MessageId
                )
                THEN 1
                ELSE 0
                END
            AS BIT);
            """;

        await connection.OpenAsync(
            cancellationToken);

        return await connection.QuerySingleAsync<bool>(
            new CommandDefinition(
                sql,
                new
                {
                    MessageId = messageId
                },
                cancellationToken:
                    cancellationToken));
    }

    public async Task MarkProcessedAsync(
        Guid messageId,
        CancellationToken cancellationToken)
    {
        using var connection =
            _connectionFactory.CreateConnection();

        const string sql = """
            INSERT INTO ProcessedMessages
                (MessageId, ProcessedAt)
            VALUES
                (@MessageId, @ProcessedAt);
            """;

        await connection.OpenAsync(
            cancellationToken);

        await connection.ExecuteAsync(
            new CommandDefinition(
                sql,
                new
                {
                    MessageId = messageId,
                    ProcessedAt = DateTime.UtcNow
                },
                cancellationToken:
                    cancellationToken));
    }
}