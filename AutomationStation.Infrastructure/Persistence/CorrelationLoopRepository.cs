using Dapper;

using AutomationStation.Application.Abstractions;
using AutomationStation.Infrastructure.Database;
using AutomationStation.Application.Models;

namespace AutomationStation.Infrastructure.Persistence
{
    public sealed class CorrelationLoopRepository(
        DbConnectionFactory connectionFactory)
        : ICorrelationLoopRepository
    {
        private readonly DbConnectionFactory _connectionFactory = connectionFactory;

        public async Task<bool> IsBlockedAsync(
            EvaluationContext context,
            CancellationToken cancellationToken)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = """
            SET XACT_ABORT ON;

            BEGIN TRANSACTION;

            DECLARE @Count INT;
            DECLARE @Blocked BIT = 0;

            SELECT @Count = COUNT(*)
            FROM AutomationHistory WITH (UPDLOCK, HOLDLOCK)
            WHERE AutomationId = @AutomationId
                AND ActorId = @ActorId
                AND TriggeredAt >= DATEADD(MINUTE, -3, SYSUTCDATETIME());

            IF @Count >= 10000
            BEGIN
                SET @Blocked = 1;
            END
            ELSE
            BEGIN
                INSERT INTO AutomationHistory (Id, AutomationId, ActorId, CorrelationId, CausationEventId, TriggeredAt)
                VALUES (@Id, @AutomationId, @ActorId, @CorrelationId, @CausationEventId, SYSUTCDATETIME());
            END

            COMMIT TRANSACTION;

            SELECT @Blocked;
            """;

            var parameters = new
            {
                Id = Guid.NewGuid(),
                context.AutomationId,
                context.CorrelationId,
                context.CausationEventId,
                ActorId = context.Actor.Id
            };

            await connection.OpenAsync(cancellationToken);

            return await connection.QuerySingleAsync<bool>(
                new CommandDefinition(
                    sql,
                    parameters,
                    cancellationToken: cancellationToken));
        }
    }
}