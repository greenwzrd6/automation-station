using AutomationStation.Application.Abstractions;
using AutomationStation.Application.Models;
using AutomationStation.Infrastructure.Database;
using Dapper;
using System.Data.Common;

namespace AutomationStation.Infrastructure.Persistence
{
    public sealed class HistoryRepository(
        DbConnectionFactory connectionFactory)
        : IHistoryRepository
    {
        private readonly DbConnectionFactory _connectionFactory = connectionFactory;

        public async Task CreateAutomationTimestampAsync(
            EvaluationContext context,
            CancellationToken cancellationToken)
        {
            using var connection = _connectionFactory.CreateConnection();

            await connection.OpenAsync(cancellationToken);

            await CreateAutomationTimestampAsync(
                context,
                connection,
                transaction: null,
                cancellationToken: cancellationToken);
        }

        internal static async Task CreateAutomationTimestampAsync(
            EvaluationContext context,
            DbConnection connection,
            DbTransaction? transaction,
            CancellationToken cancellationToken)
        {
            const string sql = """
            INSERT INTO AutomationHistory 
                (Id, AutomationId, ActorId, CorrelationId, CausationEventId, TriggeredAt)
            VALUES 
                (@Id, @AutomationId, @ActorId, @CorrelationId, @CausationEventId, SYSUTCDATETIME());
            """;

            var parameters = new
            {
                Id = Guid.NewGuid(),
                context.AutomationId,
                ActorId = context.Actor.Id,
                context.CorrelationId,
                context.CausationEventId
            };

            await connection.ExecuteAsync(
                new CommandDefinition(
                    sql,
                    parameters,
                    transaction: transaction,
                    cancellationToken: cancellationToken));
        }

        public async Task<bool> EventAlreadyProcessedAsync(
            Guid AutomationId,
            Guid CorrelationId,
            CancellationToken cancellationToken)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = """
            SELECT CAST(
               CASE WHEN EXISTS (
                    SELECT 1
                    FROM AutomationHistory
                    WHERE AutomationId = @AutomationId
                        AND CorrelationId = @CorrelationId
               )
               THEN 1
               ELSE 0
               END
            AS bit);
            """;

            var parameters = new
            {
                AutomationId,
                CorrelationId
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