using Dapper;

using AutomationStation.Application.Abstractions;
using AutomationStation.Application.Models;
using AutomationStation.Infrastructure.Database;

namespace AutomationStation.Infrastructure.Persistence
{
    /// <summary>
    /// Repository for retrieving AutomationExecutor entities from a database using a provided DbConnectionFactory.
    /// </summary>
    /// <remarks>The class is sealed and implements IAutomationExecutorRepository. Queries are executed
    /// asynchronously, honor cancellation tokens, and filter for enabled executors; Dapper is used to map results from the
    /// AutomationExecutors table.</remarks>
    /// <param name="connectionFactory">Factory that creates database connections used by the repository.</param>
    public sealed class AutomationExecutorRepository(
        DbConnectionFactory connectionFactory)
        : IAutomationExecutorRepository
    {
        private readonly DbConnectionFactory _connectionFactory = connectionFactory;

        /// <summary>
        /// Retrieves a single automation executor by ID, ignoring disabled executors.
        /// </summary>
        /// <remarks>The query filters on <c>Enabled = 1</c>, so a disabled executor is indistinguishable from one
        /// that does not exist and yields <see langword="null"/>.</remarks>
        /// <param name="executorId">The ID of the executor to look up.</param>
        /// <param name="cancellationToken">A token used to cancel opening the connection or executing the query.</param>
        /// <returns>The executor with the given ID if it exists and is enabled; otherwise <see langword="null"/>.</returns>
        public async Task<AutomationExecutor?> GetByIdAsync(
            Guid executorId,
            CancellationToken cancellationToken)
        {
            await using var connection =
                    _connectionFactory.CreateConnection();

            await connection.OpenAsync(cancellationToken);

            const string sql = """
          
                    SELECT Id, CompanyId, Enabled
                    FROM AutomationExecutors
                    WHERE Id = @ExecutorId
                      AND Enabled = 1;
            """;

            return await connection.QueryFirstOrDefaultAsync<AutomationExecutor>(
                new CommandDefinition(
                    sql,
                    new { ExecutorId = executorId },
                    cancellationToken: cancellationToken));
        }
    }
}