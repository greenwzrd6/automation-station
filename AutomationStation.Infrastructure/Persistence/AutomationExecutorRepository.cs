using AutomationStation.Application.Abstractions;
using AutomationStation.Application.Models;
using AutomationStation.Infrastructure.Database;
using Dapper;

namespace AutomationStation.Infrastructure.Persistence;

public sealed class AutomationExecutorRepository(
    DbConnectionFactory connectionFactory)
    : IAutomationExecutorRepository
{
    public async Task<AutomationExecutor?> GetByIdAsync(
        Guid executorId,
        CancellationToken cancellationToken)
    {
        await using var connection =
            connectionFactory.CreateConnection();

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