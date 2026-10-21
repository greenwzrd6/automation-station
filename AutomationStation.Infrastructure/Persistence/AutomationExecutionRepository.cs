using Dapper;

using AutomationStation.Application.Abstractions;
using AutomationStation.Infrastructure.Database;

namespace AutomationStation.Infrastructure.Persistence
{
    /// <summary>
    /// Dapper-backed repository that hands out the identifier of the execution record for an automation/event pair,
    /// creating the record on first use.
    /// </summary>
    /// <remarks>The get-or-create is performed inside a single SQL transaction that reads with
    /// <c>UPDLOCK, HOLDLOCK</c> hints, so concurrent callers for the same pair serialize on the row and all receive
    /// the same execution identifier rather than creating duplicate records.</remarks>
    /// <param name="connectionFactory">Factory that provides database connections to the automation store.</param>
    public sealed class AutomationExecutionRepository(
        DbConnectionFactory connectionFactory)
        : IAutomationExecutionRepository
    {
        private readonly DbConnectionFactory _connectionFactory = connectionFactory;

        /// <summary>
        /// Returns the existing execution identifier for the automation/event pair, inserting a new execution record
        /// with the current UTC start time when none exists yet.
        /// </summary>
        /// <param name="automationId">Identifier of the automation being executed.</param>
        /// <param name="eventId">Identifier of the event that triggered the execution; together with
        /// <paramref name="automationId"/> it uniquely identifies one execution.</param>
        /// <param name="cancellationToken">A token used to cancel the database command.</param>
        /// <returns>A task whose result is the identifier of the execution record, either pre-existing or newly
        /// inserted.</returns>
        public async Task<Guid> GetOrCreateAsync(
            Guid automationId,
            Guid eventId,
            CancellationToken cancellationToken)
        {
            await using var connection =
                _connectionFactory.CreateConnection();

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
}