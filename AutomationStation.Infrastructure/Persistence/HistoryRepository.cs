using Dapper;

using System.Data.Common;

using AutomationStation.Application.Abstractions;
using AutomationStation.Application.Models;
using AutomationStation.Infrastructure.Database;

namespace AutomationStation.Infrastructure.Persistence
{
    /// <summary>
    /// Stores and queries the AutomationHistory table that backs execution limiting and event deduplication.
    /// </summary>
    /// <remarks>Each history row records one accepted execution of an automation for an event correlation,
    /// capturing the automation id, actor id, correlation id and causation event id. Timestamps are written with the
    /// database clock (SYSUTCDATETIME()) so they compare consistently with the queries that check the rolling
    /// window. Members that take no transaction open their own connection per call from the injected
    /// <see cref="DbConnectionFactory"/>.</remarks>
    /// <param name="connectionFactory">Factory that opens connections to the automation database.</param>
    public sealed class HistoryRepository(
        DbConnectionFactory connectionFactory)
        : IHistoryRepository
    {
        private readonly DbConnectionFactory _connectionFactory = connectionFactory;

        /// <summary>
        /// Writes a history row for the given evaluation context on a new connection of its own.
        /// </summary>
        /// <remarks>The insert runs without a transaction; the TriggeredAt timestamp is produced by the database so
        /// it stays aligned with the timestamps used by limit and deduplication queries.</remarks>
        /// <param name="context">Supplies the automation id, actor id, correlation id and causation event id to
        /// store.</param>
        /// <param name="cancellationToken">A token used to cancel the insert.</param>
        /// <returns>A task that completes when the history row has been written.</returns>
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

        /// <summary>
        /// Inserts a history row on a caller-supplied connection, optionally participating in a caller's transaction.
        /// </summary>
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

        /// <summary>
        /// Checks whether an automation has already recorded an execution for the given correlation.
        /// </summary>
        /// <remarks>Used for idempotency: any history row for the automation and correlation pair counts, no matter
        /// when it was recorded. The query runs on its own connection without a transaction.</remarks>
        /// <param name="AutomationId">Identifier of the automation to look up.</param>
        /// <param name="CorrelationId">Correlation identifier of the event chain to check.</param>
        /// <param name="cancellationToken">A token used to cancel the query.</param>
        /// <returns>true when at least one matching history row exists; false otherwise.</returns>
        /// <exception cref="OperationCanceledException">
        /// The operation is canceled through <paramref name="cancellationToken"/>.
        /// </exception>
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