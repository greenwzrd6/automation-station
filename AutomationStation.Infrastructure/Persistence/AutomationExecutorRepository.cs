using AutomationStation.Application.Abstractions;
using AutomationStation.Application.Models;
using AutomationStation.Infrastructure.Database;
using Dapper;

namespace AutomationStation.Infrastructure.Persistence;

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