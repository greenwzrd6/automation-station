using Dapper;

using AutomationStation.Application.Abstractions;
using AutomationStation.Infrastructure.Database;
using AutomationStation.Application.Models;
using AutomationStation.Infrastructure.Persistence;

namespace AutomationStation.Infrastructure.RateLimiting
{
    public sealed class AutomationExecutionLimiter(
        DbConnectionFactory connectionFactory,
        HistoryRepository historyRepository)
        : IAutomationExecutionLimiter
    {
        private readonly DbConnectionFactory _connectionFactory = connectionFactory;
        private readonly HistoryRepository _historyRepository = historyRepository;

        public async Task<bool> TryRecordExecutionAsync(
            EvaluationContext context,
            CancellationToken cancellationToken)
        {
            using var connection = _connectionFactory.CreateConnection();

            await connection.OpenAsync(cancellationToken);

            await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

            const string sql = """
                SELECT CAST(
                    CASE WHEN COUNT(*) >= 10000
                        THEN 1
                        ELSE 0
                    END
                AS BIT)
                FROM AutomationHistory WITH (UPDLOCK, HOLDLOCK)
                WHERE AutomationId = @AutomationId
                  AND ActorId = @ActorId
                  AND TriggeredAt >= DATEADD(
                      MINUTE, -3, SYSUTCDATETIME());
                """;

            var parameters = new
            {
                context.AutomationId,
                ActorId = context.Actor.Id
            };

            var blocked = await connection.QuerySingleAsync<bool>(
                new CommandDefinition(
                    sql,
                    parameters,
                    transaction: transaction,
                    cancellationToken: cancellationToken));

            if (blocked)
            {
                await transaction.RollbackAsync(cancellationToken);
                return false;
            }

            await HistoryRepository.CreateAutomationTimestampAsync(
                context,
                connection,
                transaction,
                cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return true;
        }
    }
}