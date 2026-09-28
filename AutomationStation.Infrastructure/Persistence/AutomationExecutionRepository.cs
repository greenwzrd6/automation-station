using AutomationStation.Application.Abstractions;
using AutomationStation.Infrastructure.Database;
using Dapper;

namespace AutomationStation.Infrastructure.Persistence;

public sealed class AutomationExecutionRepository(
    DbConnectionFactory connectionFactory)
    : IAutomationExecutionRepository
{
    public async Task<Guid> GetOrCreateAsync(
        Guid automationId,
        Guid eventId,
        CancellationToken cancellationToken)
    {
        await using var connection =
            connectionFactory.CreateConnection();

        await connection.OpenAsync(cancellationToken);

        const string sql = """
            SET XACT_ABORT ON;

            BEGIN TRANSACTION;

            DECLARE @ExecutionId UNIQUEIDENTIFIER;

            SELECT @ExecutionId = Id
            FROM AutomationExecutions WITH (UPDLOCK, HOLDLOCK)
            WHERE AutomationId = @AutomationId
              AND EventId = @EventId;

            IF @ExecutionId IS NULL
            BEGIN
                SET @ExecutionId = NEWID();

                INSERT INTO AutomationExecutions
                (
                    Id,
                    AutomationId,
                    EventId,
                    StartedAt
                )
                VALUES
                (
                    @ExecutionId,
                    @AutomationId,
                    @EventId,
                    SYSUTCDATETIME()
                );
            END

            COMMIT TRANSACTION;

            SELECT @ExecutionId;
            """;

        return await connection.QuerySingleAsync<Guid>(
            new CommandDefinition(
                sql,
                new
                {
                    AutomationId = automationId,
                    EventId = eventId
                },
                cancellationToken: cancellationToken));
    }
}