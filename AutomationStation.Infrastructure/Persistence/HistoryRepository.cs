using Dapper;

using AutomationStation.Application.Abstractions;
using AutomationStation.Infrastructure.Database;

namespace AutomationStation.Infrastructure.Persistence
{
    public sealed class HistoryRepository(
        DbConnectionFactory connectionFactory)
        : IHistoryRepository
    {
        private readonly DbConnectionFactory _connectionFactory = connectionFactory;

        public async Task CreateAutomationTimestampAsync(
            Guid AutomationId,
            Guid CorrelationId,
            Guid? CausationEventId,
            CancellationToken CancellationToken)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = """
            INSERT INTO AutomationHistory (Id, AutomationId, CorrelationId, CausationEventId, TriggeredAt) 
            VALUES (@Id, @AutomationId, @CorrelationId, @CausationEventId, @TriggeredAt)
            """;

            var parameters = new
            {
                Id = Guid.NewGuid(),
                AutomationId,
                CorrelationId,
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

        public async Task<bool> EventAlreadyProcessedAsync(
            Guid AutomationId,
            Guid CorrelationId,
            CancellationToken CancellationToken)
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
            AS bit)
            """;

            var parameters = new
            {
                AutomationId,
                CorrelationId
            };

            await connection.OpenAsync(CancellationToken);

            return await connection.QuerySingleAsync<bool>(
                new CommandDefinition(
                    sql,
                    parameters,
                    cancellationToken: CancellationToken));
        }
    }
}