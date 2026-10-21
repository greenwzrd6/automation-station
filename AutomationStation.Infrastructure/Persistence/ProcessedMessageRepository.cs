using Dapper;

using AutomationStation.Application.Abstractions;
using AutomationStation.Infrastructure.Database;

namespace AutomationStation.Infrastructure.Persistence
{
    /// <summary>
    /// Tracks processed integration message identifiers in the ProcessedMessages table to enforce at-most-once
    /// handling.
    /// </summary>
    /// <remarks>Each method opens its own connection through <see cref="DbConnectionFactory"/>. Processing timestamps
    /// are recorded as UTC via SYSUTCDATETIME. Inserting a message identifier that is already present will violate
    /// the table's primary key and surface as a database error.</remarks>
    public sealed class ProcessedMessageRepository(
        DbConnectionFactory connectionFactory)
        : IProcessedMessageRepository
    {
        private readonly DbConnectionFactory _connectionFactory =
            connectionFactory;

        /// <summary>
        /// Determines whether a message with the given identifier has already been recorded as processed.
        /// </summary>
        /// <param name="messageId">Identifier of the message to look up in the ProcessedMessages table.</param>
        /// <param name="cancellationToken">A token used to cancel the database call.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if a
        /// matching row exists; otherwise, <see langword="false"/>.</returns>
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
                    WHERE Id = @Id
                )
                THEN 1
                ELSE 0
                END
            AS BIT);
            """;

            await connection.OpenAsync(cancellationToken);

            return await connection.QuerySingleAsync<bool>(
                new CommandDefinition(
                    sql,
                    new
                    {
                        Id = messageId
                    },
                    cancellationToken: cancellationToken));
        }

        /// <summary>
        /// Records a message identifier as processed with the current UTC timestamp.
        /// </summary>
        /// <param name="messageId">Identifier of the message to insert into the ProcessedMessages table.</param>
        /// <param name="cancellationToken">A token used to cancel the database call.</param>
        /// <returns>A task that completes when the row has been inserted.</returns>
        public async Task MarkProcessedAsync(
            Guid messageId,
            CancellationToken cancellationToken)
        {
            using var connection =
                _connectionFactory.CreateConnection();

            const string sql = """
            INSERT INTO ProcessedMessages
                (Id, ProcessedAt)
            VALUES
                (@Id, SYSUTCDATETIME());
            """;

            await connection.OpenAsync(
                cancellationToken);

            await connection.ExecuteAsync(
                new CommandDefinition(
                    sql,
                    new { Id = messageId },
                    cancellationToken: cancellationToken));
        }
    }
}
