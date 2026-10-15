using AutomationStation.Application.Abstractions;
using AutomationStation.Application.Models;
using AutomationStation.Infrastructure.Database;
using Dapper;

namespace AutomationStation.Infrastructure.Persistence;

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