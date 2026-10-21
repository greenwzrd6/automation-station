using Dapper;

using AutomationStation.Application.Abstractions;
using AutomationStation.Application.Models;
using AutomationStation.Infrastructure.Database;
using AutomationStation.Infrastructure.Persistence;

namespace AutomationStation.Infrastructure.Limiting
{
    /// <summary>
    /// Rate limiter that records automation executions in the AutomationHistory table and rejects automations that
    /// exceed the allowed number of executions per correlation within a rolling time window.
    /// </summary>
    /// <remarks>Enforcement happens inside a single serializable-style transaction: the recent-execution count is read
    /// with <c>UPDLOCK, HOLDLOCK</c> so concurrent recordings for the same automation and correlation are serialized.
    /// When the limit is exceeded nothing is recorded and the call returns <see langword="false"/>; otherwise a
    /// timestamp row is inserted and committed, returning <see langword="true"/>.</remarks>
    public sealed class AutomationExecutionLimiter(
        DbConnectionFactory connectionFactory)
        : IAutomationExecutionLimiter
    {
        private readonly DbConnectionFactory _connectionFactory = connectionFactory;

        /// <summary>
        /// Attempts to record an execution for the given automation and correlation, enforcing the per-window limit.
        /// </summary>
        /// <remarks>Counts <c>AutomationHistory</c> rows for the same automation id and correlation id triggered within
        /// the last 3 minutes. If that count has reached the limit (10,000), the transaction is rolled back and no row
        /// is written; otherwise a timestamp row for the context is inserted and the transaction is committed.</remarks>
        /// <param name="context">The automation and event pairing whose execution is being recorded; supplies the
        /// automation id and correlation id used for both counting and recording.</param>
        /// <param name="cancellationToken">A token used to cancel the connection, transaction, or queries.</param>
        /// <returns>A task that completes with <see langword="true"/> when the execution was recorded, or
        /// <see langword="false"/> when the actor has reached the limit for the current window.</returns>
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
                  AND CorrelationId = @CorrelationId
                  AND TriggeredAt >= DATEADD(
                      MINUTE, -3, SYSUTCDATETIME());
                """;

            var parameters = new
            {
                context.AutomationId,
                context.CorrelationId
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